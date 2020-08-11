# File-Picker-Sample-Implementations

This repository contains examples showing how to consume the PowerDMS File Picker. Currently only an ASP.NET Core example is provided.

## Overview

The File Picker is implemented as an IFrame, with the contents hosted at `https://filepicker.powerdms.com`.

There is a small [initialization script and stylesheet](https://github.com/PowerDMS/PowerDMS/tree/develop/src/clients/file-picker/initializer) that must be referenced on the consuming page to create and cleanup the IFrame. The initialization script will add a function called `initializePowerDmsFilePicker` to the `window` which will create the IFrame. The consuming page must have a button that will call this function.

The function `initializePowerDmsFilePicker` accepts a configuration object. One of the properties of this object is a function `onFileSelected` which is called when the user selects a PowerDMS document. When `onFileSelected` is called, the File Picker will pass information back to the consuming page, including the URL to use to get the file contents from the PowerDMS API. (See [link below](#See-also) for more details on the API.)

To see sample implementation code of the PowerDMS File Picker in further detail, visit our [Sample implementation documentation](https://github.com/PowerDMS/File-Picker-Sample-Implementations). 

## How to integrate

1. Add references to the initialization files to your page:

    ```html
    <link rel="stylesheet" href="https://filepicker.powerdms.com/initializer/powerDmsFilePicker.css" type="text/css">

    <script src="https://filepicker.powerdms.com/initializer/powerDmsFilePicker.js"></script>
    ```

2. Add a button to the page that will call `window.initializePowerDmsFilePicker(config)`.

3. Add a function that will call the PowerDMS API to get the file contents when a user makes a selection, and assign that to `config.onFileSelected` when the button is clicked.

## Configuration

`initializePowerDmsFilePicker` accepts a configuration object as a single parameter. This object has the following structure:

### JavaScript

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

### TypeScript

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
  // Restricted from 350 o 650.
  height?: number;
}
```

## Response data

### JavaScript

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

### TypeScript

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
