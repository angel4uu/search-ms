import concurrent.futures
import requests
import pyodbc
from azure.core.credentials import AzureKeyCredential
from azure.search.documents import SearchClient
from config import config  # Imports the global config instance


def fetch_catalogo_data(conn_str):
    """
    Fetches and pivots product data from the Catalogo DB.
    """
    print("Fetching and pivoting data from Catalogo DB (bifurcated EAV)...")
    products = []

    # This complex query joins two different EAV logics into one product
    query = """
            SELECT
                p."Id",
                p."Nombre",
                p."Descripcion",

                -- --- 1. Get Minimum Price from Variants ---
                MIN(v."Precio") AS "Precio",

                -- --- 2. Get Principal Image (PostgreSQL syntax) ---
                (SELECT pi."Imagen" 
                FROM "ProductoImagen" pi 
                WHERE pi."ProductoId" = p."Id" AND pi."Principal" = true
                LIMIT 1) AS "Imagen",

                -- --- 3. Get Promotion Status (PostgreSQL syntax) ---
                CASE 
                    WHEN p."PromocionId" IS NOT NULL THEN true
                    ELSE false
                END AS "TienePromocion",

                -- --- 4. Pivot PRODUCT-Level Attributes ---
                MAX(CASE WHEN a_prod."Nombre" = 'Categoría' THEN av_prod."Valor" ELSE NULL END) AS "Categoria",
                MAX(CASE WHEN a_prod."Nombre" = 'Género' THEN av_prod."Valor" ELSE NULL END) AS "Genero",
                MAX(CASE WHEN a_prod."Nombre" = 'Deporte' THEN av_prod."Valor" ELSE NULL END) AS "Deporte",
                MAX(CASE WHEN a_prod."Nombre" = 'Tipo' THEN av_prod."Valor" ELSE NULL END) AS "Tipo",
                MAX(CASE WHEN a_prod."Nombre" = 'Colección' THEN av_prod."Valor" ELSE NULL END) AS "Coleccion",

                -- --- 5. Pivot and AGGREGATE VARIANT-Level Attributes ---
                STRING_AGG(DISTINCT CASE WHEN a_var."Nombre" = 'Color' THEN av_var."Valor" ELSE NULL END, ',')
                    AS "Color", -- WITHIN GROUP is not needed for basic STRING_AGG in PG
                STRING_AGG(DISTINCT CASE WHEN a_var."Nombre" = 'Talla' THEN av_var."Valor" ELSE NULL END, ',')
                    AS "Talla"

            FROM 
                "Producto" AS p

            -- --- Joins for PRODUCT Attributes ---
            LEFT JOIN "ProductoAtributo" AS pa ON p."Id" = pa."ProductoId"
            LEFT JOIN "AtributoValor" AS av_prod ON pa."AtributoValorId" = av_prod."Id"
            LEFT JOIN "Atributo" AS a_prod ON av_prod."AtributoId" = a_prod."Id"

            -- Join only products with variants --
            INNER JOIN "Variante" AS v ON p."Id" = v."ProductoId"

            -- --- Joins for VARIANT Attributes ---
            LEFT JOIN "VarianteAtributo" AS va ON v."Id" = va."VarianteId"
            LEFT JOIN "AtributoValor" AS av_var ON va."AtributoValorId" = av_var."Id"
            LEFT JOIN "Atributo" AS a_var ON av_var."AtributoId" = a_var."Id"

            GROUP BY
                -- Group by all the core product fields
                p."Id", p."Nombre", p."Descripcion", p."PromocionId";
            """

    try:
        with pyodbc.connect(conn_str) as conn:
            cursor = conn.cursor()
            cursor.execute(query)
            rows = cursor.fetchall()

            for row in rows:
                products.append(
                    {
                        "id": str(row.Id),
                        "nombre": trim(row.Nombre),
                        "descripcion": trim(row.Descripcion),
                        "precio": float(row.Precio) if row.Precio is not None else 0.0,
                        "imagen": trim(row.Imagen),
                        "tienePromocion": row.TienePromocion is True,
                        "categoria": trim(row.Categoria),
                        "genero": trim(row.Genero),
                        "deporte": trim(row.Deporte),
                        "tipo": trim(row.Tipo),
                        "coleccion": trim(row.Coleccion),
                        "color": (
                            [trim(c) for c in row.Color.split(",")] if row.Color else []
                        ),
                        "talla": (
                            [trim(t) for t in row.Talla.split(",")] if row.Talla else []
                        ),
                        "calificacion": 0.0,
                    }
                )
    except pyodbc.Error as ex:
        print(f"Database Error: {ex}")
        raise

    print(f"Fetched {len(products)} products from DB.")
    return products


def fetch_single_rating(session, product_id):
    """
    Fetches the rating for a single product using the provided session.
    Returns a tuple (product_id, rating).
    """
    url = f"{config.resenas_api_base_url}/api/productos/{product_id}/calificacion"
    try:
        response = session.get(url, timeout=5)  # 5 second timeout
        if response.status_code == 200:
            try:
                val = float(response.text)
                return product_id, (
                    val["calificacionPromedio"]
                    if "calificacionPromedio" in val
                    else 0.0
                )
            except ValueError:
                # Handle cases where response isn't a number
                return product_id, 0.0
        elif response.status_code == 404:
            # Product has no reviews yet
            return product_id, 0.0
        else:
            print(
                f"Warning: API returned {response.status_code} for product {product_id}"
            )
            return product_id, 0.0
    except requests.RequestException as e:
        print(f"Error fetching rating for {product_id}: {e}")
        return product_id, 0.0


def enrich_products_with_ratings(products):
    """
    Orchestrates concurrent API calls to fetch ratings.
    """
    print("Fetching ratings from Reseñas MS (Concurrent)...")

    # Map for O(1) lookups
    product_map = {p["id"]: p for p in products}
    product_ids = list(product_map.keys())

    # Use a Session for connection pooling (Performance boost)
    with requests.Session() as session:
        # ThreadPoolExecutor allows us to make multiple network requests in parallel
        with concurrent.futures.ThreadPoolExecutor(max_workers=20) as executor:
            # Create a future for each product
            futures = [
                executor.submit(fetch_single_rating, session, pid)
                for pid in product_ids
            ]

            for future in concurrent.futures.as_completed(futures):
                pid, rating = future.result()
                if pid in product_map:
                    product_map[pid]["calificacion"] = rating

    print("Ratings enrichment complete.")
    return list(product_map.values())


def ingest_data():
    """Full ingestion pipeline."""
    print("Starting full data ingestion...")

    # Fetch from DB
    products = fetch_catalogo_data(config.db_conn_catalogo)

    if not products:
        print("No products found. Exiting.")
        return

    # Enrich with API data
    products_enriched = enrich_products_with_ratings(products)

    # Upload to Azure AI Search
    print(
        f"Uploading {len(products_enriched)} documents to Index '{config.azure_search_index_name}'..."
    )

    try:
        search_client = SearchClient(
            config.azure_search_endpoint,
            config.azure_search_index_name,
            AzureKeyCredential(config.azure_search_admin_key),
        )

        # Batching upload
        batch_size = 1000
        for i in range(0, len(products_enriched), batch_size):
            batch = products_enriched[i : i + batch_size]
            result = search_client.merge_or_upload_documents(documents=batch)
            print(
                f"Uploaded batch {i} to {i+len(batch)}. Success: {result[0].succeeded}"
            )

    except Exception as e:
        print(f"Error uploading to Azure Search: {e}")

    print("Data ingestion pipeline finished.")


def trim(value):
    """Helper to safely trim whitespace from strings"""
    if isinstance(value, str):
        return value.strip()
    return value


if __name__ == "__main__":
    ingest_data()
