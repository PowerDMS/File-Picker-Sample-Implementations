# PowerDMS File Picker

This repository contains examples showing how to use the PowerDMS File Picker. Currently only an ASP.NET Core example is provided.

For information about getting access to the PowerDMS File Picker, contact us at support@powerdms.com.

## Overview

The **PowerDMS File Picker** is a file selection widget that can be embedded into any web application and to provide users the ability to search for and select files from PowerDMS. 

It presents users with a familiar and consistent experience when accessing their PowerDMS content from an external source.

<br />

## Table of Contents
- [Integrating the PowerDMS File Picker](#Integrating-the-PowerDMS-File-Picker)
  - [Authenticating via the File Picker](#Authenticating-via-the-File-Picker)
  - [Displaying the PowerDMS File Picker](#Displaying-the-PowerDMS-File-Picker)
  - [Renewing the access token](#Renewing-the-access-token)
- [Common Integration Scenarios](#Common-Integration-Scenarios)
  - [Download a selected document](#Download-a-Selected-Document)
  - [Get a link to the selected document](#Link-to-a-Selected-Document)
  - [Find a previous version of the selected document that was active on a certain date](#Link-to-a-Revision-of-a-Document-on-a-Specific-Date)
- [File Picker API](#File-Picker-API)
  - [Initialization Configuration](#Initialization-Configuration)
  - [Selected File Response](#Selected-File-Response)

<br />

# Integrating the PowerDMS File Picker

There are a few high level steps to follow to integrate the File Picker into your application. 

First, you'll need to prompt the user to authenticate against PowerDMS and grant your application access on their behalf. This is accomplised by implementing an Open Id Connect Code Flow. You'll need to save the refresh token so that you can request access tokens in the future so that the user does not need to continue to authenticate. See [Authenticating via the File Picker](#Authenticating-via-the-File-Picker) for details. 

Once you've received an access token, you'll then need to display the File Picker. You'll pass in the access token and other configuration parameters. See [Displaying the PowerDMS File Picker](#Displaying-the-PowerDMS-File-Picker) for more details. 

Access tokens are valid for 1 hour. Once expired, they can be refreshed using the `refresh_token` that's returned as part of the initial authentication call. The File Picker will automatically call a configurable endpoint to generate a new access token. The details of this contract are outlined in the section [Renewing the access token](#Renewing-the-access-token). 

<br />

## Authenticating via the File Picker

The File Picker uses Open ID Connect Code Flow (OIDC Code Flow) for authentication. When the authentication function is called, we will open a new window that initiates the authentication flow. To implement authentication you'll: 

1. Reference the following stylesheet and initialization script files on the consuming page:

    ```html
    <link rel="stylesheet" href="https://filepicker.powerdms.com/initializer/powerDmsFilePicker.css" type="text/css">
    <script src="https://filepicker.powerdms.com/initializer/powerDmsFilePicker.js"></script>
    ```

2. Call the `window.PowerDms.openAuthModal` function, passing in a config object. The structure of the config object is detailed in the [Authentication Configuration section](#Authentication-Configuration).

   ```typescript
   function openAuthModal(clientConfig) {
      // Creating a state object with the username encoded. You can 
      // add whatever other information you want here
      const state = {
         hash: // A HASH YOU GENERATE AND STORE
         username: // YOUR USERNAME
      }
      var encodedState = encodeURIComponent(btoa(JSON.stringify(state)));

      var config = {
         clientId: // YOUR CLIENT ID,
         redirectUrl: // YOUR REDIRECT URL,
         state: encodedState,
      };

      window.PowerDms.openAuthModal(config);
   }
   ```
   
   ![image](https://user-images.githubusercontent.com/13018283/122817224-a84d3100-d2a5-11eb-8d43-06ee5d325665.png)
   
   ![image](https://user-images.githubusercontent.com/13018283/122818432-2bbb5200-d2a7-11eb-8272-e95d4cdb936d.png)


3.  Implement the callback to retrieve the OIDC tokens. Upon successful authentication, the user will be redirected back to the `redirectUrl` with query string parameters reflecting the code and state in the form `redirectUrl?code=${code}&state=${state}`. You will need to make a POST request to `https://accounts.powerdms.com/oauth/token` with the following form-url encoded parameters: 
      - grant_type = authorization_code
      - client_id = The client id supplied by PowerDMS
      - client_secret = The client secret supplied by PowerDMS. This is sensitive and should never be shared or returned to the    browser. 
      - code = The authorization code passed as a query string parameter after successful authentication. 
      - redirect_uri = Your redirect url. This must be in the list of allowed URIs for this client.  

      The following is a sample HTTP request:
      ```http
      POST https://accounts.powerdms.com/oauth/token
      Content-Type: application/x-www-form-urlencoded

      grant_type=authorization_code&client_id=YOUR_CLIENT_ID&client_secret=YOUR_CLIENT_SECRET&code=AUTHORIZATION_CODE&redirect_uri=https://YOUR_APP/callback
      ```
      
      This is an example response: 
      ```http
      HTTP/1.1 200 OK
      Content-Type: application/json
      {
      "access_token":"eyJz93a...k4laUWw",
      "refresh_token":"GEbRxBN...edjnXbL",
      "id_token":"eyJ0XAi...4faeEoQ",
      "token_type":"Bearer",
      "expires_in":86400
      }
      ```

      After retrieving the tokens, you'll need to save your refresh tokens for later retrieval, and then complete the auth flow by redirecting the user to the File Picker with the correct tokens. The full redirect url will be: `https://filepicker.powerdms.com/auth-finalize?$access_token=YOUR_ACCESS_TOKEN&client_id=YOUR_CLIENT_ID&id_token=YOUR_ID_TOKEN&locale=YOUR_LOCALE&redirect_url=https://YOUR_APP/callback`

      Below is sample C# code implementing this step: 

      ```c#
      [HttpGet, Route("callback")]
      public async Task<ActionResult> Callback(string code, string state, string error, string error_description)
      {
         if (!string.IsNullOrEmpty(error))
         {
               return new JsonResult(new {
                  error,
                  error_description
               });
         }

         var url = $"{AuthServerHost}/oauth/token";
         var content = new FormUrlEncodedContent(new Dictionary<string, string>
         {
               ["grant_type"] = "authorization_code",
               ["client_id"] = ClientID,
               ["client_secret"] = ClientSecret,
               ["code"] = code,
               ["redirect_uri"] = $"http://localhost:8008/callback"
         });

         var httpClient = new HttpClient();
         var response = await httpClient.PostAsync(url, content);

         if (response.IsSuccessStatusCode)
         {
               var responseJson = await response.Content.ReadAsStringAsync();
               var tokenResponse = JsonSerializer.Deserialize<TokenResponse>(responseJson);

               // save refresh token
               MemoryCache.Default["refreshToken"] = tokenResponse.refresh_token;

               var responseUrl = $"{FilePickerHost}/auth-finalize?" +
                  $"access_token={Uri.EscapeDataString(tokenResponse.access_token)}&" +
                  $"client_id={Uri.EscapeDataString(ClientID)}&" +
                  $"id_token={Uri.EscapeDataString(tokenResponse.id_token)}&" +
                  $"redirect_url={Uri.EscapeDataString("http://localhost:8008/callback")}";

               return new RedirectResult(responseUrl);
         }
         return null;
      }

      public class TokenResponse
      {
         public string access_token { get; set; }

         public string refresh_token { get; set; }

         public string id_token { get; set; }

         public string token_type { get; set; }
      }
      ``` 

<br />

## Displaying the PowerDMS File Picker

At a high level, the PowerDMS File Picker is implemented as an iFrame that gets embedded into your application. Follow these three simple steps to get started.

<br />

1. Create a function that will be called when a user makes a selection:

   ```javascript
   function displaySelection(response) {

      var json = JSON.stringify(response);
      var message = 'Selection: \n' + json;

      alert(message);
   }
   ```

   The `response` will include a list of selected documents that gets passed on to the consuming page, including the URLs that can be used to get the files from the [PowerDMS API](https://apidocs.powerdms.com).

2. Call the function `openFilePicker` from the consuming page using a button:

   ```javascript
   function receiveMessage(event) {

      if (event.origin !== 'https://filepicker.powerdms.com')
      {
         return;
      }

      var message = event.data;

      if (message.type === 'DMS_FILEPICKER_SENDTOKENSTOHOST') {
         const tokens = message.data;

         const config = {
               accessToken: tokens.accessToken,
               idToken: tokens.idToken,
               tokenRefreshUrl: window.location.origin + '/refresh',
               onSelection: displayResponse,
         };

         window.PowerDms.openFilePicker(config);
      }
   }
   window.addEventListener('message', receiveMessage, false);
   ```
   
   ![image](https://user-images.githubusercontent.com/13018283/122818572-5f967780-d2a7-11eb-9d4f-39ef7087b929.png)


   The `configuration` provided to `openFilePicker` must include the `API Key` provided by PowerDMS for your application and the function you previously created to receive the `onSelection` callback when the user selects documents.

<br />

## Renewing the access token

Our access tokens expire after 1 hour. Expired access tokens will need to be renewed. The File Picker automatically renews access tokens that are no longer valid by calling the endpoint specified on the `tokenRefreshUrl?` configuration option. This is an endpoint that you must implement that conforms to a contract that the File Picker expects. The endpoint must be a POST that accepts the idToken and the partner's username. This endpoint should look up the refresh token that is tied to the user id and call the `https://accounts.powerdms.com/oauth/token` endpoint. The form of this call should be: 

   ```http
   POST https://accounts.powerdms.com/oauth/token
   Content-Type: application/x-www-form-urlencoded

   grant_type=refresh_token&client_id=YOUR_CLIENT_ID&client_secret=YOUR_CLIENT_SECRET&refresh_token=YOUR_REFRESH_TOKEN
   ```

   Here is an example endpoint in C# using Web API: 

   ```c#
   [HttpPost, Route("refresh")]
   public async Task<ActionResult> Refresh([FromQuery(Name = "id_token")] string idToken, string username)
   {
      // get last saved refresh token
      var refreshToken = MemoryCache.Default["refreshToken"].ToString();

      var url = $"{AuthServerHost}/oauth/token";
      var content = new FormUrlEncodedContent(new Dictionary<string, string>
      {
            ["grant_type"] = "refresh_token",
            ["client_id"] = ClientID,
            ["client_secret"] = ClientSecret,
            ["refresh_token"] = refreshToken
      });

      var httpClient = new HttpClient();
      var response = await httpClient.PostAsync(url, content);

      if (!response.IsSuccessStatusCode)
      {
            throw new Exception($"Failed to get tokens ({response.ReasonPhrase})");
      }

      var json = await response.Content.ReadAsStringAsync();

      // see structure of token response below
      var tokenResponse = JsonSerializer.Deserialize<TokenResponse>(json);

      return new OkObjectResult(new
      {
            AccessToken = tokenResponse.access_token,
            IdToken = tokenResponse.id_token
      });
   }
   ```

# Common Integration Scenarios

The PowerDMS File Picker returns metadata corresponding to the files a user selected. Using the [PowerDMS API](https://apidocs.powerdms.com), your application can extend this functionality and build custom integration experiences. 

This section provides examples for some common scenarios.

<br />

## Download a Selected Document

The PowerDMS File Picker [`SelectedFileResponse`](#Selected-File-Response) object contains the information necessary to download the files corresponding to the the documents selected by the user. 

Using the `documentId` and `revisionId` from a selected document, call the `Get File by ID` endpoint using an authorized request:

```http
GET https://api.powerdms.com/v1/documents/{documentId}/revisions/{revisionId}/content
```

For your convenience, the url used to download the file for the selected document is also included as the `contentUrl` property.

See the specific endpoint documentation for [Getting Files](https://apidocs.powerdms.com/#026ef757-9125-4ce2-b0cc-dbd52cb0a94c) in the [PowerDMS API](https://apidocs.powerdms.com/).

<br />

## Link to a Selected Document

You can construct a link to the selected document that allows an authenticated user to view the latest version of the selected document within PowerDMS.com with the following format:

```http
https://powerdms.com/link/{siteKey}/document/?id={documentId}
``` 

- `{siteKey}` is the unique identifier that corresponds to the organization in PowerDMS the user is logged into. *This is not a part of the File Picker response and will need to be prompted for in a different way.*

- `{documentId}` is the id of the selected document. This is provided by the `documentId` property on the selected file object.

<br />

For example, to link to document `201893` in site `AcmeCorp`, you'll construct a link that looks like the following:

```http
https://powerdms.com/link/AcmeCorp/document/?id=201893
```

<br />

## Link to a Revision of a Document on a Specific Date

You can also link to a specific version of a selected document that was active on a certain date in PowerDMS.com. This will require you to request published revisions on a specific date using the API.

Call the `Published Revisions for Documents in date` endpoint using an authorized request and specify the date the revisions should have been published for a set of documents.

The following example would provide the revisions that were active on `October 5th, 2019` for documents `15560`, `28915`, and `78137`:

```json
POST https://api.powerdms.com/v1/documents/past-published-revisions

{
   "targetDate": "2019-10-05T04:00:00.000Z",
   "documentIds": [15560, 28915, 78137]
}
```

*The timezone in `targetDate` is optional. The site's default timezone will be used if not included.*

This endpoint will return the active revision at the `targetDate` for each `documentId` provided. The example response for the above call would be:

```json
{
   "data": [
   {
      "documentId": "15560",
      "isPublicationFound": true,
      "publishedRevision": {
         "documentRevisionId": "39405",
         "deepLinkUri": "https://powerdms.com/link/AcmeCorp/revision/?ID=39405"
      }
   },
   {
      "documentId": "28915",
      "isPublicationFound": false,
      "publishedRevision": null
   },
   {
      "documentId": "78137",
      "isPublicationFound": false,
      "publishedRevision": null
   }   
],
"error": null
}
```

If there was no published revision for the document on the specific date, the this will be `false` and the `publishedRevision` will be *null*.

Using the above response, the link to a specific revision would be provided in the `deepLinkUri` property:

```http
https://powerdms.com/link/AcmeCorp/revision/?ID=39405
```

<br />

See the specific endpoint documentation for [Getting Revisions](https://apidocs.powerdms.com/#4b5e1bec-3aaa-4807-9b30-a7f6c6e9c2d3) in the [PowerDMS API](https://apidocs.powerdms.com/).

<br />

# File Picker API

`PowerDms` is a top level namespace that will house all PowerDMS File Picker API classes.

## Authentication Configuration
The `window.PowerDms.openAuthModal` function accepts a `configuration` object as a single parameter. This object has the following structure (in TypeScript):

```typescript
type AuthModalConfig = {
   // Client Id supplied by PowerDMS 
   clientId: string,
   
   // Url to your endpoint that will handle the code flow 
   redirectUrl: string,

   // Optional width of the auth modal
   width?: number;

   // Optional height of the auth modal
   height?: number;

   // The anti-csrf state parameter that will be passed back to redirectUrl after the user 
   // successfully authenticates. We recommend encoding your user's id so that when retrieving 
   // tokens, you can save tokens by user id.  
   state?: string;
}
```

## Initialization Configuration

The `initializePowerDmsFilePicker` function accepts a `configuration` object as a single parameter. This object has the following structure (in TypeScript):

```typescript
type FilePickerConfig = {
  // The access token from the OIDC Code Flow.
  accessToken?: string;

  // The desired height of the file picker, in pixels.
  // Restricted from 350 to 650.
  height?: number;

  // The id token from the OIDC Code Flow.
  idToken?: string

  // The local. Defaults to en-us.
  locale?: string;

  // A callback that is called when a user makes a selection.
  onSelection: (response: SelectionResponse) => void;

  // This is the url that the File Picker will hit when the access token expires in order to 
  // refresh the token. 
  tokenRefreshUrl?: string;

  // The desired width of the file picker, in pixels.
  // Restricted from 566 to 1051.
  width?: number;
}
```

## Selected File Response

When files are selected, the `onSelection` callback is invoked with metadata about the selected files. The metadata has the following structure: 

```typescript
type SelectionResponse = {
  selectedFiles: ResponseFileInfo[];
}

type ResponseFileInfo = {
  documentId: string;
  documentName: string;
  breadcrumbs: ResponseParentFolder[];
  revisionId: string;
  revisionStatus: ObjectStatus;

  // This will be the URL of an API endpoint to get the file contents.
  contentUrl: string;
}

enum ObjectStatus {
  Draft = 'draft',
  Published = 'published',
  Archived = 'archived',
}

type ResponseParentFolder = {
  id: number;
  name: string;
}
```

Here is an example of the response you'll get from selected 3 documents: 

```json
{
   "selectedFiles":[
      {
         "documentId":"13211",
         "documentName":"Conduct Policy (Public & Published)",
         "breadcrumbs":[
            {
               "id":"13269",
               "name":"Policies"
            }
         ],
         "revisionId":"38917",
         "revisionStatus":"Publication",
         "contentUrl":"http://powerdms.com/v1/documents/13211/revisions/38917/content"
      },
      {
         "documentId":"13752",
         "documentName":"Sexual Harassment Policy",
         "breadcrumbs":[
            {
               "id":"13269",
               "name":"Policies"
            }
         ],
         "revisionId":"35674",
         "revisionStatus":"Publication",
         "contentUrl":"http://powerdms.com/v1/documents/13752/revisions/35674/content"
      },
      {
         "documentId":"17896",
         "documentName":"Procedural Policy (Draft)",
         "breadcrumbs":[
            {
               "id":"13269",
               "name":"Policies"
            }
         ],
         "revisionId":"32154",
         "revisionStatus":"Draft",
         "contentUrl":"http://powerdms.com/v1/documents/17896/revisions/32154/content"
      }
   ]
}
```
