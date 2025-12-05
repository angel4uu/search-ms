import os
from dotenv import load_dotenv
from typing import Final

# Load environment variables from .env file at the top level
load_dotenv()


class Config:
    """
    Loads all environment variables from the .env file.
    Provides a central, type-safe way to access configuration.
    Raises an error if a required variable is missing.
    """

    def __init__(self):
        self.azure_search_endpoint: Final[str] = self._get_required_env(
            "AZURE_SEARCH_SERVICE_ENDPOINT"
        )
        self.azure_search_admin_key: Final[str] = self._get_required_env(
            "AZURE_SEARCH_ADMIN_API_KEY"
        )
        self.azure_search_index_name: Final[str] = self._get_required_env(
            "AZURE_SEARCH_INDEX_NAME"
        )
        self.db_conn_catalogo: Final[str] = self._get_required_env("DB_CONN_CATALOGO")

        self.resenas_api_base_url: Final[str] = self._get_required_env(
            "RESENAS_API_BASE_URL"
        )

    def _get_required_env(self, var_name: str) -> str:
        """Helper to get an env var and raise an error if it's missing."""
        value = os.getenv(var_name)
        if not value:
            raise ValueError(
                f"Error: Environment variable '{var_name}' is not set. Please check your .env file."
            )
        return value


# Create a single, global instance of the config that scripts can import
try:
    config = Config()
except ValueError as e:
    print(e)
    exit(1)
