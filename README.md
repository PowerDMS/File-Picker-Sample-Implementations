# PowerDMS File Picker

This repository contains examples showing how to use the PowerDMS File Picker. Currently only an ASP.NET Core example is provided.

For information about getting access to the PowerDMS File Picker, contact us at support@powerdms.com.

## Overview

The **PowerDMS File Picker** is a file selection widget that can be embedded into any web application and to provide users the ability to search for and select files from PowerDMS. 

It presents users with a familiar and consistent experience when accessing their PowerDMS content from an external source.

<br />

## Table of Contents

- [How to use the PowerDMS File Picker](#Using-the-file-picker)
- [Common Integration Scenarios](#Common-Integration-Scenarios)
  - [Download a selected document](#Download-a-Selected-Document)
  - [Get a link to the selected document](#Link-to-a-Selected-Document)
  - [Find a previous version of the selected document that was active on a certain date](#Link-to-a-Revision-of-a-Document-on-a-Specific-Date)
- [File Picker API](#File-Picker-API)
  - [Initialization Configuration](#Initialization-Configuration)
  - [Selected File Response](#Selected-File-Response)

<br />

# Using the File Picker

At a high level, the PowerDMS File Picker is implemented as an iFrame that gets embedded into your application. Follow these three simple steps to get started.

<br />

1. Reference the following stylesheet and initialization script files on the consuming page:

    ```html
    <link rel="stylesheet" href="https://filepicker.powerdms.com/initializer/powerDmsFilePicker.css" type="text/css">

    <script src="https://filepicker.powerdms.com/initializer/powerDmsFilePicker.js"></script>
    ```

   The initialization script will add a function called `initializePowerDmsFilePicker` to the `window` which will style and create the iFrame.

2. Create a function that will be called when a user makes a selection:

   ```javascript
   function displaySelection(response) {

      var json = JSON.stringify(response);
      var message = 'Selection: \n' + json;

      alert(message);
   }
   ```

   The `response` will include a list of selected documents that gets passed on to the consuming page, including the URLs that can be used to get the files from the [PowerDMS API](https://apidocs.powerdms.com).

3. Call the function `initializePowerDmsFilePicker` from the consuming page using a button:

   ```javascript
   function openFilePicker() {

      var config = {
         apiKey: 'your-api-key',
         onSelection: displaySelection,
      };

      window.PowerDms.initializePowerDmsFilePicker(config);
   }
   ```

   The `configuration` provided to `initializePowerDmsFilePicker` must include the `API Key` provided by PowerDMS for your application and the function you previously created to receive the `onSelection` callback when the user selects documents.

<br />

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

See the specific endpoint documentation for [Getting Revisions](https://apidocs.powerdms.com/#026ef757-9125-4ce2-b0cc-dbd52cb0a94c) in the [PowerDMS API](https://apidocs.powerdms.com/).

<br />

# File Picker API

`PowerDms` is a top level namespace that will house all PowerDMS File Picker API classes. This includes the following calls:


## Initialization Configuration

The `initializePowerDmsFilePicker` function accepts a `configuration` object as a single parameter. This object has the following structure (in TypeScript):

```typescript
type FilePickerConfig = {
  // The API key given to your organization from PowerDMS.
  apiKey: string;

  // A callback that is called when a user makes a selection.
  onSelection: (response: SelectionResponse) => void;

  // The desired width of the file picker, in pixels.
  // Restricted from 566 to 1051.
  width?: number;

  // The desired height of the file picker, in pixels.
  // Restricted from 350 to 650.
  height?: number;
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
