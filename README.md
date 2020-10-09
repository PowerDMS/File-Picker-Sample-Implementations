# File-Picker-Sample-Implementations

This repository contains examples showing how to consume the PowerDMS File Picker. Currently only an ASP.NET Core example is provided.

## Overview

The File Picker is implemented as an iFrame, with the contents hosted at `https://filepicker.powerdms.com`.

There is a small initialization script and stylesheet that must be referenced on the consuming page to create and cleanup the iFrame. The initialization script will add a function called `initializePowerDmsFilePicker` to the `window` which will create the iFrame. The consuming page must have a button that will call this function.

The function `initializePowerDmsFilePicker` accepts a configuration object. One of the properties of this object is a function `onFileSelected` which is called when the user selects a PowerDMS document. When `onFileSelected` is called, the File Picker will pass information back to the consuming page, including the URL to use to get the file contents from the PowerDMS API. (See [link below](#See-also) for more details on the API.)

To see sample implementation code of the PowerDMS File Picker in further detail, visit our sample implementation [HTML](/aspnetcore/FilePickerSample/wwwroot/index.html) and [JavaScript](/aspnetcore/FilePickerSample/wwwroot/js/site.js) files. 

## How to integrate

1. Add references to the initialization files to your page:

    ```html
    <link rel="stylesheet" href="https://filepicker.powerdms.com/initializer/powerDmsFilePicker.css" type="text/css">

    <script src="https://filepicker.powerdms.com/initializer/powerDmsFilePicker.js"></script>
    ```

2. Add a button to the page that will call `window.PowerDms.initializePowerDmsFilePicker(config)`.

3. Add a function that will call the PowerDMS API to get the file contents when a user makes a selection, and assign that to `config.onFileSelected` when the button is clicked.

## Configuration
`PowerDms` is a top level namespace that will house all PowerDms api calls. This includes the following calls:


### `initializePowerDmsFilePicker`
`initializePowerDmsFilePicker` accepts a configuration object as a single parameter. This object has the following structure:

#### JavaScript

```javascript
{
  // String, required
  // The API key given to your organization from PowerDMS.
  apiKey,

  // Function, required
  // A callback that is called when a user makes a selection.
  onSelection,

  // Number, optional
  // The desired width of the file picker, in pixels.
  // Restricted from 566 to 1051.
  width,

  // Number, optional
  // The desired height of the file picker, in pixels.
  // Restricted from 350 o 650.
  height
}
```

#### TypeScript

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

### Response data

#### JavaScript

```javascript
// Response
{
  // Array of file info objects
  selectedFiles
}

// File info
{
  // Number
  documentId,

  // String
  documentName,

  // Array of parent folder objects
  breadcrumbs,

  // Number
  revisionId,

  // String ('draft', 'published', or 'archived')
  revisionStatus,

  // String
  // This will be the URL of an API endpoint to get the file contents.
  contentUrl
}

// Parent folder
{
  // Number
  id,

  // String
  name
}
```

#### TypeScript

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

## API usage

For more details on how to use the PowerDMS API, see its [documentation](https://api.powerdms.com/openapi/ui/).

The endpoint to get files contents is `/documents/{documentId}/revisions/{revisionId}/content` in the _DocumentRevisionFiles_ section.


## Doing More with File Picker

Now that you have the PowerDMS File Picker up and running and you're able to select a document, let's take a look at the things that you can do with the information you've received back from your selection(s). 

### Deep Links

In some cases, you might need to add a deep link to your site or get a reference to a particular document. A deep link is a hyperlink that will direct your users to a specific document within your organization's PowerDMS site in the PowerDMS Document Viewer. You might be most familiar with it as an internal link from your PowerDMS site:

![Internal Link PowerDMS](https://s3.us-west-2.amazonaws.com/secure.notion-static.com/26c3d1dd-7f24-4963-95d3-e8b448961561/Untitled.png?X-Amz-Algorithm=AWS4-HMAC-SHA256&X-Amz-Credential=AKIAT73L2G45O3KS52Y5%2F20201009%2Fus-west-2%2Fs3%2Faws4_request&X-Amz-Date=20201009T170454Z&X-Amz-Expires=86400&X-Amz-Signature=1dadc4335fb7ee8570e8f5dff70cde2fa1f01c430dd604ae3b6134e5b21076ee&X-Amz-SignedHeaders=host&response-content-disposition=filename%20%3D%22Untitled.png%22)

#### Creating a deep link for current published documents

Once you've selected all the documents you need via the PowerDMS File Picker, you'll get a JSON response. It should look similar to this:

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
         "contentUrl":"http://api.dev.powerdms.net/v1/documents/8/revisions/8/content"
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
         "contentUrl":"http://api.dev.powerdms.net/v1/documents/1/revisions/1/content"
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
         "contentUrl":"http://api.dev.powerdms.net/v1/documents/2/revisions/2/content"
      }
   ]
}
```

In this example, we have selected three documents. To create a deep link, we would start with the following format (where `siteKey` is the site key you would use to sign in to your PowerDMS site and `documentId` is the id of the document in the received response):

```jsx
https://powerdms.com/link/<siteKey>/document/?id=<documentId>
```

To make a deep link for the first document with a `siteKey` of AcmeCorp, we'd create the following:

```html
https://powerdms.com/link/AcmeCorp/document/?id=8
```

#### Creating deep links for a previously active document

*This will need to be updated based on possible File Picker changes for date. At the time of writing this, date has not yet been implemented as a return value for the user in File Picker.* 

Let's consider the following JSON:

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
         "revisionDate": "2020-10-08T18:25:43.511Z"
         "revisionStatus":"Publication",
         "contentUrl":"http://api.dev.powerdms.net/v1/documents/8/revisions/8/content"
      }
   ]
}
```

Notice, we have access to the `revisionDate` as well as the `revisionId`. 

To create a deep link for a particular revision, we'd follow this format:

```html
[https://powerdms.com/link/](https://powerdms.com/link/)<sitekey>/revision/?ID=<revisionId>
```

We'll use the same `siteKey` as the previous example of `AcmeCorp` to create a revision deep link

```html
https://powerdms.com/link/AcmeCorp/revision/?id=8
```

To see a revision from a particular date we'd follow the same structure but add the date to the end of the link;

```html
https://powerdms.com/link/AcmeCorp/revision/?id=8&date=<revisionDate>
```
