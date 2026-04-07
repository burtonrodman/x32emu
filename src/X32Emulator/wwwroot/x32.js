'use strict';

const state = {
    channels: Array.from({length: 32}, (_, i) => ({
        name: `Ch ${String(i+1).padStart(2,'0')}`,
        fader: 0.75,
        on: true,
        pan: 0.5,
        color: 0,
        solo: false,
        selected: false
    })),
    buses: Array.from({length: 16}, (_, i) => ({
        name: `Bus ${String(i+1).padStart(2,'0')}`,
        fader: 0.75,
        on: true,
        pan: 0.5
    })),
    matrices: Array.from({length: 6}, (_, i) => ({
        name: `Mtx ${String(i+1).padStart(2,'0')}`,
        fader: 0.75,
        on: true
    })),
    dcas: Array.from({length: 8}, (_, i) => ({
        name: `DCA ${i+1}`,
        fader: 0.75,
        on: true
    })),
    fxrtn: Array.from({length: 8}, (_, i) => ({
        name: `FxRtn ${i+1}`,
        fader: 0.75,
        on: true
    })),
    main: { fader: 0.75, on: true },
    currentPage: 'ch1'
};

let connection = null;
let meterInterval = null;

function faderToDb(val) {
    if (val < 0.001) return '-inf';
    if (val < 0.5) return ((val - 0.5) * 80).toFixed(1) + 'dB';
    return ((val - 0.75) * 40).toFixed(1) + 'dB';
}

function faderToMidi(val) {
    return Math.round(val * 1023);
}

function midiToFader(midi) {
    return midi / 1023;
}

function createChannelStrip(idx, label, pathPrefix) {
    const strip = document.createElement('div');
    strip.className = 'channel-strip';
    strip.id = `strip-${pathPrefix}-${idx}`;

    const chNum = document.createElement('div');
    chNum.className = 'ch-number';
    chNum.textContent = String(idx + 1).padStart(2, '0');

    const nameLabel = document.createElement('div');
    nameLabel.className = 'strip-label';
    nameLabel.id = `name-${pathPrefix}-${idx}`;
    nameLabel.textContent = label;
    nameLabel.title = 'Click to rename';

    const pan = document.createElement('input');
    pan.type = 'range';
    pan.className = 'pan-knob';
    pan.min = 0; pan.max = 127; pan.value = 64;
    pan.title = 'Pan';
    pan.addEventListener('input', () => {
        const panVal = pan.value / 127;
        sendParam(getPath(pathPrefix, idx, 'pan'), panVal.toFixed(4));
    });

    const meterCont = document.createElement('div');
    meterCont.className = 'meter-container';
    const meterL = document.createElement('div');
    meterL.className = 'meter';
    meterL.id = `meter-${pathPrefix}-${idx}-l`;
    meterCont.appendChild(meterL);

    const faderCont = document.createElement('div');
    faderCont.className = 'fader-container';
    const fader = document.createElement('input');
    fader.type = 'range';
    fader.className = 'vertical-fader';
    fader.min = 0; fader.max = 1023; fader.value = 768;
    fader.id = `fader-${pathPrefix}-${idx}`;
    fader.addEventListener('input', () => {
        const fVal = midiToFader(parseInt(fader.value));
        sendParam(getPath(pathPrefix, idx, 'fader'), fVal.toFixed(4));
        updateFaderVal(pathPrefix, idx, fVal);
    });
    faderCont.appendChild(fader);

    const faderVal = document.createElement('div');
    faderVal.className = 'fader-value';
    faderVal.id = `fval-${pathPrefix}-${idx}`;
    faderVal.textContent = '0dB';

    const btnRow = document.createElement('div');
    btnRow.className = 'strip-buttons';

    const muteBtn = document.createElement('button');
    muteBtn.className = 'mute-btn';
    muteBtn.textContent = 'M';
    muteBtn.id = `mute-${pathPrefix}-${idx}`;
    muteBtn.title = 'Mute';
    muteBtn.addEventListener('click', () => toggleMute(pathPrefix, idx));

    const soloBtn = document.createElement('button');
    soloBtn.className = 'solo-btn';
    soloBtn.textContent = 'S';
    soloBtn.id = `solo-${pathPrefix}-${idx}`;
    soloBtn.title = 'Solo';
    soloBtn.addEventListener('click', () => toggleSolo(pathPrefix, idx));

    btnRow.appendChild(muteBtn);
    btnRow.appendChild(soloBtn);

    strip.appendChild(chNum);
    strip.appendChild(nameLabel);
    strip.appendChild(pan);
    strip.appendChild(meterCont);
    strip.appendChild(faderCont);
    strip.appendChild(faderVal);
    strip.appendChild(btnRow);

    return strip;
}

function getPath(prefix, idx, prop) {
    const num = String(idx + 1).padStart(2, '0');
    switch (prefix) {
        case 'ch': return `/ch/${num}/mix/${prop}`;
        case 'bus': return `/bus/${num}/mix/${prop}`;
        case 'mtx': return `/mtx/${num}/mix/${prop}`;
        case 'dca': return `/dca/${idx + 1}/${prop === 'fader' ? 'fader' : (prop === 'on' ? 'on' : prop)}`;
        case 'fxrtn': return `/fxrtn/${num}/mix/${prop}`;
        default: return `/${prefix}/${num}/mix/${prop}`;
    }
}

function updateFaderVal(prefix, idx, val) {
    const el = document.getElementById(`fval-${prefix}-${idx}`);
    if (el) el.textContent = faderToDb(val);
}

function updateFader(prefix, idx, val) {
    const fader = document.getElementById(`fader-${prefix}-${idx}`);
    if (fader) fader.value = faderToMidi(val);
    updateFaderVal(prefix, idx, val);
}

function updateMute(prefix, idx, on) {
    const btn = document.getElementById(`mute-${prefix}-${idx}`);
    if (btn) {
        btn.classList.toggle('active', !on);
    }
}

function updateChannelName(prefix, idx, name) {
    const el = document.getElementById(`name-${prefix}-${idx}`);
    if (el) el.textContent = name;
}

function toggleMute(prefix, idx) {
    let currentOn;
    switch(prefix) {
        case 'ch': currentOn = state.channels[idx].on; break;
        case 'bus': currentOn = state.buses[idx].on; break;
        case 'dca': currentOn = state.dcas[idx].on; break;
        default: currentOn = true;
    }
    const newOn = !currentOn;
    sendParam(getPath(prefix, idx, 'on'), newOn ? '1' : '0');
}

function toggleSolo(prefix, idx) {
    if (prefix === 'ch') {
        state.channels[idx].solo = !state.channels[idx].solo;
        const btn = document.getElementById(`solo-${prefix}-${idx}`);
        if (btn) btn.classList.toggle('active', state.channels[idx].solo);
    }
}

function sendParam(path, value) {
    if (connection && connection.state === signalR.HubConnectionState.Connected) {
        connection.invoke('SetParam', path, String(value)).catch(console.error);
    }
}

function renderPage(page) {
    state.currentPage = page;
    const area = document.getElementById('channelArea');
    area.innerHTML = '';

    document.querySelectorAll('.page-btn').forEach(btn => {
        btn.classList.toggle('active', btn.dataset.page === page);
    });

    let strips = [];
    if (page === 'ch1') strips = Array.from({length:8}, (_,i) => ({prefix:'ch', idx:i, label:state.channels[i].name}));
    else if (page === 'ch9') strips = Array.from({length:8}, (_,i) => ({prefix:'ch', idx:i+8, label:state.channels[i+8].name}));
    else if (page === 'ch17') strips = Array.from({length:8}, (_,i) => ({prefix:'ch', idx:i+16, label:state.channels[i+16].name}));
    else if (page === 'ch25') strips = Array.from({length:8}, (_,i) => ({prefix:'ch', idx:i+24, label:state.channels[i+24].name}));
    else if (page === 'fxrtn') strips = Array.from({length:8}, (_,i) => ({prefix:'fxrtn', idx:i, label:state.fxrtn[i].name}));
    else if (page === 'buses') strips = Array.from({length:8}, (_,i) => ({prefix:'bus', idx:i, label:state.buses[i].name}));
    else if (page === 'matrix') strips = Array.from({length:6}, (_,i) => ({prefix:'mtx', idx:i, label:state.matrices[i].name}));
    else if (page === 'dca') strips = Array.from({length:8}, (_,i) => ({prefix:'dca', idx:i, label:state.dcas[i].name}));

    strips.forEach(({prefix, idx, label}) => {
        const strip = createChannelStrip(idx, label, prefix);
        area.appendChild(strip);
        let fval = 0.75, on = true;
        switch(prefix) {
            case 'ch': fval = state.channels[idx].fader; on = state.channels[idx].on; break;
            case 'bus': fval = state.buses[idx].fader; on = state.buses[idx].on; break;
            case 'mtx': fval = state.matrices[idx].fader; on = state.matrices[idx].on; break;
            case 'dca': fval = state.dcas[idx].fader; on = state.dcas[idx].on; break;
            case 'fxrtn': fval = state.fxrtn[idx].fader; on = state.fxrtn[idx].on; break;
        }
        updateFader(prefix, idx, fval);
        updateMute(prefix, idx, on);
    });
}

function handleParamChanged(path, value) {
    const parts = path.split('/').filter(Boolean);
    const fval = parseFloat(value);
    const ival = parseInt(value);

    if (parts[0] === 'ch' && parts.length >= 4) {
        const idx = parseInt(parts[1]) - 1;
        if (idx < 0 || idx >= 32) return;
        if (parts[2] === 'mix') {
            if (parts[3] === 'fader') { state.channels[idx].fader = fval; updateFader('ch', idx, fval); }
            else if (parts[3] === 'on') { state.channels[idx].on = ival !== 0; updateMute('ch', idx, ival !== 0); }
        } else if (parts[2] === 'config' && parts[3] === 'name') {
            state.channels[idx].name = value;
            updateChannelName('ch', idx, value);
        }
    } else if (parts[0] === 'bus' && parts.length >= 4) {
        const idx = parseInt(parts[1]) - 1;
        if (idx < 0 || idx >= 16) return;
        if (parts[2] === 'mix') {
            if (parts[3] === 'fader') { state.buses[idx].fader = fval; updateFader('bus', idx, fval); }
            else if (parts[3] === 'on') { state.buses[idx].on = ival !== 0; updateMute('bus', idx, ival !== 0); }
        }
    } else if (parts[0] === 'dca' && parts.length >= 3) {
        const idx = parseInt(parts[1]) - 1;
        if (idx < 0 || idx >= 8) return;
        if (parts[2] === 'fader') { state.dcas[idx].fader = fval; updateFader('dca', idx, fval); }
        else if (parts[2] === 'on') { state.dcas[idx].on = ival !== 0; updateMute('dca', idx, ival !== 0); }
    } else if (parts[0] === 'main' && parts[1] === 'st' && parts.length >= 4) {
        if (parts[2] === 'mix') {
            if (parts[3] === 'fader') {
                state.main.fader = fval;
                const mf = document.getElementById('main-fader');
                if (mf) mf.value = faderToMidi(fval);
            } else if (parts[3] === 'on') {
                state.main.on = ival !== 0;
                const mb = document.getElementById('main-mute');
                if (mb) mb.classList.toggle('active', !state.main.on);
            }
        }
    }
}

function simulateMeters() {
    document.querySelectorAll('.meter').forEach(m => {
        const h = Math.random() * 60;
        m.style.height = h + '%';
    });
}

async function loadSceneList() {
    try {
        const res = await fetch('/api/scenes');
        const scenes = await res.json();
        if (scenes.length === 0) { alert('No scenes found in /data/scenes'); return; }
        const choice = prompt('Available scenes:\n' + scenes.join('\n') + '\n\nEnter scene name to load:');
        if (choice) {
            await fetch(`/api/scenes/${encodeURIComponent(choice)}/load`, {method:'POST'});
            document.getElementById('sceneDisplay').textContent = choice;
        }
    } catch(e) { console.error(e); }
}

function initSignalR() {
    connection = new signalR.HubConnectionBuilder()
        .withUrl('/hub')
        .withAutomaticReconnect([0, 1000, 2000, 5000, 10000])
        .build();

    connection.on('ParamChanged', handleParamChanged);

    connection.onreconnecting(() => {
        const el = document.getElementById('connStatus');
        el.textContent = 'Reconnecting...';
        el.className = 'connection-status disconnected';
    });

    connection.onreconnected(() => {
        const el = document.getElementById('connStatus');
        el.textContent = 'Connected';
        el.className = 'connection-status connected';
    });

    connection.onclose(() => {
        const el = document.getElementById('connStatus');
        el.textContent = 'Disconnected';
        el.className = 'connection-status disconnected';
    });

    async function start() {
        try {
            await connection.start();
            const el = document.getElementById('connStatus');
            el.textContent = 'Connected';
            el.className = 'connection-status connected';
            loadInitialState();
        } catch(e) {
            console.error('SignalR error:', e);
            setTimeout(start, 3000);
        }
    }
    start();
}

async function loadInitialState() {
    try {
        const res = await fetch('/api/state');
        const data = await res.json();
        if (data.channels) {
            data.channels.forEach((ch, i) => {
                state.channels[i].name = ch.config && ch.config.name ? ch.config.name : state.channels[i].name;
                state.channels[i].fader = ch.mix && ch.mix.fader != null ? ch.mix.fader : 0.75;
                state.channels[i].on = ch.mix && ch.mix.on != null ? ch.mix.on : true;
                state.channels[i].pan = ch.mix && ch.mix.pan != null ? ch.mix.pan : 0.5;
            });
        }
        if (data.buses) {
            data.buses.forEach((b, i) => {
                state.buses[i].fader = b.mix && b.mix.fader != null ? b.mix.fader : 0.75;
                state.buses[i].on = b.mix && b.mix.on != null ? b.mix.on : true;
            });
        }
        if (data.mainStereo) {
            state.main.fader = data.mainStereo.fader != null ? data.mainStereo.fader : 0.75;
            state.main.on = data.mainStereo.on != null ? data.mainStereo.on : true;
        }
        renderPage(state.currentPage);
        const mf = document.getElementById('main-fader');
        if (mf) mf.value = faderToMidi(state.main.fader);
        const mb = document.getElementById('main-mute');
        if (mb) mb.classList.toggle('active', !state.main.on);
    } catch(e) { console.error('Failed to load initial state:', e); }
}

// Page buttons
document.querySelectorAll('.page-btn').forEach(btn => {
    btn.addEventListener('click', () => renderPage(btn.dataset.page));
});

// Main fader
document.getElementById('main-fader').addEventListener('input', function() {
    const fVal = midiToFader(parseInt(this.value));
    state.main.fader = fVal;
    sendParam('/main/st/mix/fader', fVal.toFixed(4));
    const el = document.getElementById('main-fader-val');
    if (el) el.textContent = faderToDb(fVal);
});

// Main mute
document.getElementById('main-mute').addEventListener('click', function() {
    state.main.on = !state.main.on;
    this.classList.toggle('active', !state.main.on);
    sendParam('/main/st/mix/on', state.main.on ? '1' : '0');
});

// Init
renderPage('ch1');
initSignalR();
meterInterval = setInterval(simulateMeters, 100);
