# File-Picker-Sample-Implementations

This repository contains examples showing how to consume the PowerDMS File Picker. Currently only an ASP.NET Core example is provided.

## Overview

The File Picker is implemented as an IFrame, with the contents hosted at `https://filepicker.powerdms.com`.

There is a small initialization script and stylesheet that must be referenced on the consuming page to create and cleanup the IFrame. The initialization script will add a function called `initializePowerDmsFilePicker` to the `window` which will create the IFrame. The consuming page must have a button that will call this function.

The function `initializePowerDmsFilePicker` accepts a configuration object. One of the properties of this object is a function `onFileSelected` which is called when the user selects a PowerDMS document. When `onFileSelected` is called, the File Picker will pass information back to the consuming page, including the URL to use to get the file contents from the PowerDMS API. (See [link below](#See-also) for more details on the API.)

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

```javascript
{
  apiKey,         // string,   required
  onFileSelected, // function, required
  width,          // number,   optional
  height          // number,   optional
}
```

If you are familiar with TypeScript, the properties have these types:

```typescript
{
  apiKey: string,
  onFileSelected: (fileRoute: string) => void,
  width?: number,
  height?: number
}
```

### `apiKey`

- Required, string
- This is the API key given to your organization from PowerDMS.

### `onFileSelected`

- Required, function
- This is a callback that is called when a user selects a file to import.
- The string parameter is the URL that can be used to acquire the actual file contents.
- See [link below](#See-also) for more details on the API.

### `width`

- Optional, number
- The desired width of the file picker, in pixels. This is restricted from 566 to 1051.

### `height`

- Optional, number
- The desired height of the file picker, in pixels. This is restricted from 350 to 650.

## See also

For more details on how to use the PowerDMS API, see its [documentation](https://api.powerdms.com/openapi/ui/), specifically the endpoint `/documents/{documentId}/revisions/{revisionId}/content` in the _DocumentRevisionFiles_ section.
