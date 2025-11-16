from azure.core.credentials import AzureKeyCredential
from azure.search.documents.indexes import SearchIndexClient
from azure.core.exceptions import ResourceNotFoundError
from config import config  # Imports your secrets
from index_schema import get_index_definition, INDEX_NAME  # Imports your schema
import time  # Import the time module


def create_index():
    """
    Deletes and recreates the Azure AI Search index.
    Includes delays to prevent race conditions with ingestion.
    """
    # Load configuration
    endpoint = config.azure_search_endpoint
    key = config.azure_search_admin_key
    index_name = INDEX_NAME

    # Create the SearchIndexClient
    try:
        print(f"Connecting to {endpoint}...")
        index_client = SearchIndexClient(endpoint, AzureKeyCredential(key))
    except Exception as e:
        print(f"Failed to connect to Azure Search service: {e}")
        return

    # Delete the index if it exists
    try:
        print(f"Checking if index '{index_name}' exists...")
        index_client.get_index(index_name)

        print("Index exists. Deleting...")
        index_client.delete_index(index_name)

        # Wait 30 seconds for the deletion to fully complete on Azure's side
        print("Waiting 30 seconds for Azure to process the deletion...")
        time.sleep(30)

    except ResourceNotFoundError:
        print("Index does not exist. Proceeding to creation.")
    except Exception as ex:
        print(f"An unexpected error occurred during index check/deletion: {ex}")
        raise ex

    # Get the complete index definition
    index = get_index_definition()

    # Create the index
    try:
        print(f"Creating new index '{index_name}'...")
        index_client.create_index(index)

        # Wait 30 seconds for the index to be fully built and filters to be mapped
        print("Waiting 30 seconds for Azure to build the index...")
        time.sleep(30)
    except ResourceNotFoundError:
        print("Index does not exist. Proceeding to creation.")

    except Exception as ex:
        print(f"An unexpected error occurred during index creation: {ex}")
        raise ex

    print(f"Index '{index_name}' created successfully.")


if __name__ == "__main__":
    create_index()
