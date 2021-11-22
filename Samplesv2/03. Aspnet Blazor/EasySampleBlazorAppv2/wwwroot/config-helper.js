function getJSON(url, callback) {
    var req = new XMLHttpRequest();
    req.open('GET', url, true);
    req.onload = function () {
        var status = req.status;
        console.log(`status: ${status}`);
        if (status === 200) { callback(null, req.response); } else { callback(status, req.response); }
    };
    req.send();
    return req;
};

function getEnvSuffix(url) {
    let re = /(\w+)[-:$]/;
    let result = re.exec(url);
    var env = result[result.length - 1];
    console.log(`env: ${env}`);
    let envSuffix = 'Local';
    switch (env) {
        case 'localhost': { envSuffix = 'Local'; break; }
        case 'dev': { envSuffix = 'Development'; break; }
        case 'qa': { envSuffix = 'QA'; break; }
        case 'prod': { envSuffix = 'Production'; break; }
    }
    return envSuffix;
};

