import React, { useEffect, useState } from 'react';
import { Home } from './components/Home';
import { Layout } from './components/Layout';
import './custom.css';
import '../node_modules/ka-table/style.css';

import { EventEmitter } from 'eventemitter3';
import { Connecting } from './components/Connecting';
export default function App() {
    const [socket, setSocket] = useState(null);
    const [isConnected, setConnected] = useState(false);
    const [socketObserver, setSocketObserver] = useState(null);

    useEffect(() => {
        const protocol = window.location.protocol === "https:" ? "wss:" : "ws:";
        let host = window.location.host;

        if (host === 'localhost:44407')
            host = `${window.location.hostname}:7160` // Set port in launchSettings.json 

        const socket = new WebSocket(`${protocol}//${host}/wsconn`);

        const socketObserver = new EventEmitter();
        socket.addEventListener('open', (event) => {
            setSocket(socket);
            setConnected(true);
        });

        socket.addEventListener('message', (event) => {
            socketObserver.emit('message', event);
        });

        socket.addEventListener('close', (event) => {
            socketObserver.emit('close', event);
            setConnected(false);
        });
        socket.addEventListener('error', (event) => {
            socketObserver.emit('error', event);
            setConnected(false);
        });

        setSocketObserver(socketObserver);
    }, []);

    function send(data) {
        if (!isConnected)
            return;

        socket.send(JSON.stringify(data));
    }

    return (
        <Layout>
            {isConnected ? <Home Send={(data) => send(data)} socketObserver={socketObserver} /> : <Connecting />}
        </Layout>
    );
}
