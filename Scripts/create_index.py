from azure.core.credentials import AzureKeyCredential
from azure.search.documents.indexes import SearchIndexClient
from azure.search.documents.indexes.models import (
    SearchIndex
)
from azure.core.exceptions import ResourceNotFoundError
from config import config 

def create_index():
    """
    Deletes and recreates the Azure AI Search index based on the C# model.
    """
    # Load configuration
    endpoint = config.azure_search_endpoint
    key = config.azure_search_admin_key
    index_name = config.azure_search_index_name

    # Create the SearchIndexClient
    try:
      print(f"Connecting to {endpoint}...")
      index_client = SearchIndexClient(endpoint, AzureKeyCredential(key))
    except Exception as e:
      print(f"Failed to connect to Azure Search service: {e}")
      return

    # Define the index schema
    fields = config.index_fields
    index = SearchIndex(name=index_name, fields=fields)

    # Delete the index if it exists
    try:
        print(f"Checking if index '{index_name}' exists...")
        # Attempting to get the index will raise an error if it doesn't exist
        index_client.get_index(index_name) 
        
        # If the above line succeeds, the index exists, so delete it
        print("Index exists. Deleting...")
        index_client.delete_index(index_name)

    # Catch the specific exception raised when the resource is not found
    except ResourceNotFoundError:
        print("Index does not exist. Proceeding to creation.")
    except Exception as ex:
        print(f"An unexpected error occurred during index check/deletion: {ex}")
        raise ex
        
    # Create the index
    try:
        print("Creating new index...")
        index_client.create_index(index)
        print(f"Index '{index_name}' created successfully.")
    except Exception as e:
        print(f"Failed to create index: {e}")

if __name__ == "__main__":
    create_index()