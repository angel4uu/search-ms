from azure.search.documents.indexes.models import (
    SearchIndex,
    SimpleField,
    SearchField,
    SearchableField,
    SearchFieldDataType,
    Suggester,
    ScoringProfile,
    TextWeights,
    MagnitudeScoringFunction,
    MagnitudeScoringParameters,
)

# Define the index name
INDEX_NAME = "productos-index"

# Define the fields
FIELDS = [
    SimpleField(name="id", type=SearchFieldDataType.String, key=True, filterable=True),
    # Searchable fields
    SearchableField(
        name="nombre",
        type=SearchFieldDataType.String,
        sortable=True,
        # --- ADDED for Suggester ---
        filterable=True,
        facetable=True,
    ),
    SearchableField(name="descripcion", type=SearchFieldDataType.String, hidden=True),
    # Filterable and retrievable fields
    SimpleField(
        name="precio",
        type=SearchFieldDataType.Double,
        filterable=True,
        sortable=True,
    ),
    SimpleField(name="imagen", type=SearchFieldDataType.String),
    SimpleField(
        name="tienePromocion", type=SearchFieldDataType.Boolean, filterable=True
    ),
    SimpleField(
        name="calificacion",
        type=SearchFieldDataType.Double,
        filterable=True,
        sortable=True,
    ),
    # Filterable and not retrievable string fields
    SearchableField(
        name="categoria",
        type=SearchFieldDataType.String,
        filterable=True,
        facetable=True,
        hidden=True,
    ),
    SearchableField(
        name="genero",
        type=SearchFieldDataType.String,
        filterable=True,
        facetable=True,
        hidden=True,
    ),
    SearchableField(
        name="deporte",
        type=SearchFieldDataType.String,
        filterable=True,
        facetable=True,
        hidden=True,
    ),
    SearchableField(
        name="tipo",
        type=SearchFieldDataType.String,
        filterable=True,
        facetable=True,
        hidden=True,
    ),
    SearchableField(
        name="coleccion",
        type=SearchFieldDataType.String,
        filterable=True,
        facetable=True,
        hidden=True,
    ),
    # Filterable and not retrievable string array fields
    SearchField(
        name="color",
        type=SearchFieldDataType.Collection(SearchFieldDataType.String),
        filterable=True,
        facetable=True,
        hidden=True,
        searchable=True,
    ),
    SearchField(
        name="talla",
        type=SearchFieldDataType.Collection(SearchFieldDataType.String),
        filterable=True,
        facetable=True,
        hidden=True,
        searchable=True,
    ),
]

# Auto-suggest configuration
SUGGESTERS = [Suggester(name="sg", source_fields=["nombre"])]

# Boost matches in 'nombre', 'calificacion' and description
SCORING_PROFILES = [
    ScoringProfile(
        name="boost_rating_and_name",
        # Boost matches in 'nombre' higher than other fields
        text_weights=TextWeights(weights={"nombre": 5, "descripcion": 2}),
        functions=[
            # Boost products with a higher rating
            MagnitudeScoringFunction(
                field_name="calificacion",
                boost=7,
                parameters=MagnitudeScoringParameters(
                    boosting_range_start=3.5,
                    boosting_range_end=5,
                ),
            )
        ],
    )
]


# Function to get the complete index definition
def get_index_definition() -> SearchIndex:
    """
    Creates the complete SearchIndex object from all schema components.
    """
    return SearchIndex(
        name=INDEX_NAME,
        fields=FIELDS,
        suggesters=SUGGESTERS,
        scoring_profiles=SCORING_PROFILES,
        default_scoring_profile="boost_rating_and_name",
    )
