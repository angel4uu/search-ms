import pyodbc
from azure.core.credentials import AzureKeyCredential
from azure.search.documents import SearchClient
from config import config  # Imports the global config instance


def fetch_catalogo_data(conn_str):
    """
    Fetches and pivots product data from the Catalogo DB to prepare for indexing.

    This query is designed for a bifurcated EAV schema:
    1. Gets PRODUCT-level attributes (Category, Gender...)
    2. Gets and AGGREGATES VARIANT-level attributes (Color, Talla...)
    3. Gets the principal 'imagen' from Producto_Imagen.
    4. Gets 'tienePromocion' status from the Producto table.
    5. Gets the MINIMUM 'precio' from all associated Variantes.
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
                    WHEN p."IdPromocion" IS NOT NULL THEN true
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

            -- --- Joins for VARIANT Attributes (CRITICAL for Precio, Color, Talla) ---
            LEFT JOIN "Variante" AS v ON p."Id" = v."ProductoId"
            LEFT JOIN "VarianteAtributo" AS va ON v."Id" = va."VarianteId"
            LEFT JOIN "AtributoValor" AS av_var ON va."AtributoValorId" = av_var."Id"
            LEFT JOIN "Atributo" AS a_var ON av_var."AtributoId" = a_var."Id"

            GROUP BY
                -- Group by all the core product fields
                p."Id", p."Nombre", p."Descripcion", p."IdPromocion";
            """

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
                    "precio": float(row.Precio) if row.Precio is not None else None,
                    "imagen": trim(row.Imagen),
                    "tienePromocion": row.TienePromocion is True,
                    # These fields come from the pivoted query
                    "categoria": trim(row.Categoria),
                    "genero": trim(row.Genero),
                    "deporte": trim(row.Deporte),
                    "tipo": trim(row.Tipo),
                    "coleccion": trim(row.Coleccion),
                    # Split the aggregated strings back into arrays for the index
                    "color": (
                        [trim(c) for c in row.Color.split(",")] if row.Color else []
                    ),
                    "talla": (
                        [trim(t) for t in row.Talla.split(",")] if row.Talla else []
                    ),
                }
            )

    print(f"Fetched and pivoted {len(products)} products.")
    return products


def fetch_reseñas_data():
    """
    Calls the Reseñas microservice to get average ratings per product.
    """
    print("Fetching average ratings from Reseñas MS...")
    rating_dict = {}

    return rating_dict


def ingest_data():
    """Full ingestion pipeline."""

    print("Starting full data ingestion...")
    print(f"Connected to Catalogo DB: {config.db_conn_catalogo}")

    # Get data from all sources
    products = fetch_catalogo_data(config.db_conn_catalogo)
    reseñas = fetch_reseñas_data()

    print("Merging data from all sources...")

    # Augment products with rating data
    for product in products:
        # Default to 5.0 cause not implemented yet
        product["calificacion"] = 5.0

    # Set up the SearchClient
    search_client = SearchClient(
        config.azure_search_endpoint,
        config.azure_search_index_name,
        AzureKeyCredential(config.azure_search_admin_key),
    )

    # Upload documents
    search_client.merge_or_upload_documents(documents=products)

    print("Data ingestion completed successfully.")


def trim(value):
    """Helper to safely trim whitespace from strings"""
    if isinstance(value, str):
        return value.strip()
    return value


if __name__ == "__main__":
    ingest_data()
