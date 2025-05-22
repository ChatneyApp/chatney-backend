; (() => {
    // expectingMessage is set to true
    // if the user has just submitted a message
    // and so we should scroll the next message into view when received.
    let expectingMessage = false

    const connectFailureThreshold = 10;
    let connectionFailureCounter = 0;
    let timeout;
    function dial() {
        const ip = document.getElementById('ip_connect');
        const conn = new WebSocket(`ws://${ip.value}/subscribe`)

        conn.addEventListener('close', ev => {
            appendLog(`WebSocket Disconnected code: ${ev.code}, reason: ${ev.reason}`, true)
            if (ev.code !== 1001) {
                connectionFailureCounter++;

                appendLog('Reconnecting in 1s', true)
                timeout = setTimeout(dial, 1000)

                if (connectionFailureCounter >= connectFailureThreshold) {
                    clearTimeout(timeout);
                }
            }
        })
        conn.addEventListener('open', ev => {
            console.info('websocket connected')
            appendLog('Connected to WS');
        })

        // This is where we handle messages received.
        conn.addEventListener('message', ev => {
            if (typeof ev.data !== 'string') {
                console.error('unexpected message type', typeof ev.data)
                return
            }
            const p = appendLog(ev.data)
            if (expectingMessage) {
                p.scrollIntoView()
                expectingMessage = false
            }
        })
    }


    const messageLog = document.getElementById('message-log')
    const publishForm = document.getElementById('publish-form')
    const messageInput = document.getElementById('message-input')

    // appendLog appends the passed text to messageLog.
    function appendLog(text, error) {
        const p = document.createElement('p')
        // Adding a timestamp to each message makes the log easier to read.
        p.innerText = `${new Date().toLocaleTimeString()}: ${text}`
        if (error) {
            p.style.color = 'red'
            p.style.fontStyle = 'bold'
        }
        messageLog.append(p)
        return p
    }
    appendLog('Submit a message to get started!')

    // onsubmit publishes the message from the user when the form is submitted.
    publishForm.onsubmit = async ev => {
        ev.preventDefault()

        const msg = messageInput.value
        if (msg === '') {
            return
        }
        messageInput.value = ''

        expectingMessage = true
        try {
            const resp = await fetch('http://localhost:64799/publish', {
                method: 'POST',
                body: msg,
            })
            if (resp.status !== 202) {
                throw new Error(`Unexpected HTTP Status ${resp.status} ${resp.statusText}`)
            }
        } catch (err) {
            appendLog(`Publish failed: ${err.message}`, true)
        }
    }

    document.getElementById('connectTo').addEventListener('click', dial);
})()