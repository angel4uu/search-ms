import os
from dotenv import load_dotenv
from typing import Final
from azure.search.documents.indexes.models import (
    SimpleField,
    SearchableField,
    SearchFieldDataType
)

# Load environment variables from .env file at the top level
load_dotenv()

class Config:
    """
    Loads all environment variables from the .env file.
    Provides a central, type-safe way to access configuration.
    Raises an error if a required variable is missing.
    """
    def __init__(self):
        
        self.azure_search_endpoint: Final[str] = self._get_required_env("AZURE_SEARCH_SERVICE_ENDPOINT")
        self.azure_search_admin_key: Final[str] = self._get_required_env("AZURE_SEARCH_ADMIN_API_KEY")
        self.azure_search_index_name: Final[str] = self._get_required_env("AZURE_SEARCH_INDEX_NAME")
        
        self.index_fields: Final[list] = [
        SimpleField(name="id", type=SearchFieldDataType.String, key=True, filterable=True),
        # Searchable fields
        SearchableField(name="nombre", type=SearchFieldDataType.String, sortable=True),
        SearchableField(name="descripcion", type=SearchFieldDataType.String, hidden=True),
        # Filterable and retrievable fields
        SimpleField(name="precio", type=SearchFieldDataType.Double, filterable=True, sortable=True),
        SimpleField(name="imagen", type=SearchFieldDataType.String),
        SimpleField(name="tienePromocion", type=SearchFieldDataType.Boolean, filterable=True),
        SimpleField(name="calificacion", type=SearchFieldDataType.Double, filterable=True, sortable=True),
        # Filterable and not retrievable string fields
        SimpleField(name="categoria", type=SearchFieldDataType.String, filterable=True, facetable=True, hidden=True),
        SimpleField(name="genero", type=SearchFieldDataType.String, filterable=True, facetable=True, hidden=True),
        SimpleField(name="deporte", type=SearchFieldDataType.String, filterable=True, facetable=True, hidden=True),
        SimpleField(name="tipo", type=SearchFieldDataType.String, filterable=True, facetable=True, hidden=True),
        SimpleField(name="coleccion", type=SearchFieldDataType.String, filterable=True, facetable=True, hidden=True),
        # Filterable and not retrievable string array fields
        SimpleField(name="color", type=SearchFieldDataType.Collection(SearchFieldDataType.String), filterable=True, facetable=True, hidden=True),
        SimpleField(name="talla", type=SearchFieldDataType.Collection(SearchFieldDataType.String), filterable=True, facetable=True, hidden=True),
    ]

    def _get_required_env(self, var_name: str) -> str:
        """Helper to get an env var and raise an error if it's missing."""
        value = os.getenv(var_name)
        if not value:
            raise ValueError(f"Error: Environment variable '{var_name}' is not set. Please check your .env file.")
        return value

# Create a single, global instance of the config that scripts can import
try:
    config = Config()
except ValueError as e:
    print(e)
    exit(1)