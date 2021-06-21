(function() {
    console.log("Javascript loaded.");
})();

function openAuthModal() {
    // Creating a state object with the username encoded. You can 
    // add whatever other information you want here
    const state = {
        hash: 'fake-hash',
        username: 'fake-username'
    }
    var encodedState = encodeURIComponent(btoa(JSON.stringify(state)));

    var config = {
        clientId: 'your-client-id',
        redirectUrl: window.location.origin + '/callback',
        state: encodedState,
    };

    window.PowerDms.openAuthModal(config);
}

function displayResponse(response) {
    var json = JSON.stringify(response, null, 2);
    var message = 'FilePicker says: \n' + json;
    alert(message);
}

function receiveMessage(event) {
    console.log(event);

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