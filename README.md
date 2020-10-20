# File-Picker-Sample-Implementations

This repository contains examples showing how to consume the PowerDMS File Picker. Currently only an ASP.NET Core example is provided.

## Overview

The PowerDMS File Picker is a widget that provides to users the ability to search for and select their PowerDMS documents from within another site using a familiar user experience 

<br />

## Table of Contents

- [Integrating the File Picker](#How-to-integrate)
- [Common Integration Scenarios](#Common-Integration-Scenarios)
  - [Download the selected document](#Downloading-the-Selected-Document)
  - [Get a link to the selected document](#Linking-to-the-Selected-Document)
  - [Find a previous version of the selected document that was active on a certain date](#Active-Revision-on-a-Specific-Date)
- [File Picker API](#File-Picker-API)
  - [Initialization Configuration](#Initialization-Configuration)
  - [Selected File Response](#Selected-File-Response)

<br />

## How to integrate
At a high level, you do a few things to integrate the File Picker: 
1. Add references to the initialization files to your page:

    ```html
    <link rel="stylesheet" href="https://filepicker.powerdms.com/initializer/powerDmsFilePicker.css" type="text/css">

    <script src="https://filepicker.powerdms.com/initializer/powerDmsFilePicker.js"></script>
    ```
2. Prompt the user to authenticate.

3. Open the File Picker.

     Add a button to the page that will call `window.PowerDms.initializePowerDmsFilePicker(config)`.

4. Handle the selection result.

     Add a function that will call the PowerDMS API to get the file contents when a user makes a selection, and assign that to `config.onFileSelected` when the button is clicked.

<br />


## Common Integration Scenarios
The File Picker returns metadata about selected files. You may need to orchestrate additional API calls to achieve the intended integration experience. This section provides examples for some common scenarios.     

<br />

### Downloading the Selected Document
The File Picker selection object contains all of the information necessary to download the current revision of the selected file. To do so, call the `/documents/{documentId}/revisions/{revisionId}/content` endpoint using the `documentId` and `revisionId` from the selected file. For convenience, the correct link for the selected document is passed as the `contentUrl` property on the selection result. See the specific endpoint documentation (link tbd) for details.   

<br />

### Linking to the Selected Document
You can construct a link to the selected document that allows an authenticated user to view the latest version of the selected document. The format of the link is `https://powerdms.com/link/{siteKey}/document/?id={documentId}` where:
- `{siteKey}` is the PowerDMS Site Key that a user logging into PowerDMS specifies to identify the correct site to log into. This is not a part of the File Picker response and will need to be prompted for in a different way. 
- `{documentId}` is the id of the selected document. This maps to the `documentId` property on the selected file response object. 

For example, to link to document 20 in site AcmeCorp, you'll construct a link `https://powerdms.com/link/AcmeCorp/document/?id=20`.

<br />

### Active Revision on a Specific Date
To link to the previous version of the selected document that was active on a certain, date, you will need to: 

Call `POST /documents/past-published-revisions`. In the message body, specify the date at which you want the active revision, and the ids of the documents whose active revisions you want at the specified date. For example, to find the revisions of documents 10, 15, and 77 that were active on October 5th, 2019, you would make a call to `POST /documents/past-published-revisions` with a request body of 
```json
{
   "targetDate": "2019-10-05T04:00:00.000Z",
   "documentIds": [10, 15, 77]
}
```
The timezone part of the date object is optional, if not included, then the site's default timezone will be used. 

For each `documentId` passed, this endpoint will return the active revision at the targetDate. An example response for the above call could be: 
```json
{
   "data": [
   {
      "documentId": "10",
      "isPublicationFound": true,
      "publishedRevision": {
         "documentRevisionId": "2",
         "deepLinkUri": "https://powerdms.com/link/AcmeCorp/revision/?ID=2"
      }
   },
   {
      "documentId": "15",
      "isPublicationFound": false,
      "publishedRevision": null
   },
   {
      "documentId": "77",
      "isPublicationFound": false,
      "publishedRevision": null
   }   
],
"error": null
}
```

Note the `isPublicationFound` boolean. If there was no published revision for the document on the specific date, the this will be `false` and the `publishedRevision` will be null. 

Using the data from the above response, you can download document contents or construct a link to a specific revision as normal. 

<br />

## File Picker API 

`PowerDms` is a top level namespace that will house all PowerDms api calls. This includes the following calls:


### Initialization Configuration
`initializePowerDmsFilePicker` accepts a configuration object as a single parameter. This object has the following structure (in TypeScript):

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

### Selected File Response

When files are selected, we invoke the `onSelection` callback with metadata about the selected files. The metadata has the following structure: 

```typescript
type SelectionResponse = {
  selectedFiles: ResponseFileInfo[];
}

type ResponseFileInfo = {
  documentId: number;
  documentName: string;
  breadcrumbs: ResponseParentFolder[];
  revisionId: number;
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
         "documentId":"8",
         "documentName":"Conduct Policy (Public & Published)",
         "breadcrumbs":[
            {
               "id":"13",
               "name":"Policies"
            }
         ],
         "revisionId":"8",
         "revisionStatus":"Publication",
         "contentUrl":"http://powerdms.com/v1/documents/8/revisions/8/content"
      },
      {
         "documentId":"1",
         "documentName":"Sexual Harassment Policy",
         "breadcrumbs":[
            {
               "id":"13",
               "name":"Policies"
            }
         ],
         "revisionId":"1",
         "revisionStatus":"Publication",
         "contentUrl":"http://powerdms.com/v1/documents/1/revisions/1/content"
      },
      {
         "documentId":"2",
         "documentName":"Procedural Policy (Draft)",
         "breadcrumbs":[
            {
               "id":"13",
               "name":"Policies"
            }
         ],
         "revisionId":"2",
         "revisionStatus":"Draft",
         "contentUrl":"http://powerdms.com/v1/documents/2/revisions/2/content"
      }
   ]
}
```
