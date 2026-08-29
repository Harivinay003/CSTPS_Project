// Shared fragment initializers for SLD sub-pages.
// Each fragment should include a canvas with id conventions (e.g. mpccCanvas).
// Exported initializers: window.initMpcc, window.initU1Rabh, ...
(function () {
    // registry per canvas to keep track of live-updatable items and pollers
    const _liveRegistry = {};

    // helper: format numeric value for display
    function _fmt(val) {
        if (val === null || val === undefined) return "----";
        if (Number.isFinite(val)) {
            // choose decimals based on magnitude
            return (Math.abs(val) >= 100 ? val.toFixed(0) : val.toFixed(2));
        }
        return String(val);
    }

    // register a serial device area on a canvas so poller updates it every second
    function registerSerialDeviceArea(canvas, ctx, serDevId, area) {
        const _liveRegistry = {};
        if (!serDevId) return;
        const cid = canvas.id || ('canvas_' + Math.random().toString(36).slice(2));
        if (!_liveRegistry[cid]) {
            _liveRegistry[cid] = {
                canvas,
                ctx,
                items: {}, // serDevId -> area
                timer: null
            };
        }
        _liveRegistry[cid].items[serDevId] = area;
        //console.log(`Registered live area for serial device ${serDevId} on canvas ${cid}`, area);
        // start polling if not running
        if (!_liveRegistry[cid].timer) {
            _liveRegistry[cid].timer = setInterval(async () => {
                // fetch each serial device's live values and update only its area
                const entries = Object.entries(_liveRegistry[cid].items);
                for (const [sId, a] of entries) {
                    try {
                        const resp = await fetch(`/api/live/serial/${sId}`);
                        if (!resp.ok) continue;
                        const data = await resp.json(); // array of { paramId, value, name, symbol }

                        // build symbol map for quick lookup (symbol and name, case-insensitive)
                        const map = {};
                        data.forEach(d => {
                            if (d.symbol) {
                                if (d.symbol.toString().toLowerCase() === 'kw') {
                                    map[d.symbol.toString().toLowerCase()] = d.value / 1000;
                                }
                                else {
                                    map[d.symbol.toString().toLowerCase()] = d.value;
                                }
                            }
                        });
                        // compute values for KW, I, V, PF
                        const kw = map['KW'] ?? map['kw'] ?? null;
                        const i = map['I-avg'] ?? map['i-avg'] ?? null;
                        const v = map['V LL Avg'] ?? map['v ll avg'] ?? null;
                        const pf = map['PF'] ?? map['pf'] ?? null;


                        // clear area and draw updated values
                        const ctx2 = _liveRegistry[cid].ctx;
                        ctx2.save();
                        // clear rectangle that covers the four value lines
                        ctx2.fillStyle = 'white';
                        ctx2.fillRect(a.clearLeft - 2, a.clearTop - 2, a.clearWidth + 4, a.clearHeight + 4);

                        // redraw labels (if caller expects labels remain; draw values on top)
                        // draw values
                        ctx2.textAlign = "left";
                        ctx2.textBaseline = "middle";
                        ctx2.font = '600 11px Arial';
                        ctx2.fillStyle = 'red';
                        ctx2.fillText(_fmt(kw), a.x + 0, a.y + 0);   // KW position
                        ctx2.fillText(_fmt(i), a.x + 0, a.y + 15);  // I position
                        ctx2.fillText(_fmt(v), a.x + 0, a.y + 30);  // V position
                        ctx2.fillText(_fmt(pf), a.x + 0, a.y + 45); // PF position
                        ctx2.restore();
                    } catch (err) {
                        // network/parsing errors ignored silently
                        // console.error('live update error', err);
                    }
                }
            }, 1000);
        }
    }

    // register a serial device area on a canvas so poller updates it every second
    function registerFieldDeviceArea(canvas, ctx, serDevId, area) {
        const _liveRegistry = {};
        if (!serDevId) return;
        const cid = canvas.id || ('canvas_' + Math.random().toString(36).slice(2));
        if (!_liveRegistry[cid]) {
            _liveRegistry[cid] = {
                canvas,
                ctx,
                items: {}, // serDevId -> area
                timer: null
            };
        }
        _liveRegistry[cid].items[serDevId] = area;
        //console.log(`Registered live area for serial device ${serDevId} on canvas ${cid}`, area);
        // start polling if not running
        if (!_liveRegistry[cid].timer) {
            _liveRegistry[cid].timer = setInterval(async () => {
                // fetch each serial device's live values and update only its area
                const entries = Object.entries(_liveRegistry[cid].items);
                for (const [sId, a] of entries) {
                    try {
                        const resp = await fetch(`/api/live/field/${sId}`);
                        if (!resp.ok) continue;
                        const data = await resp.json(); // array of { paramId, value, name, symbol }

                        // build symbol map for quick lookup (symbol and name, case-insensitive)
                        const map = {};
                        data.forEach(d => {
                            if (d.symbol) map[d.symbol.toString().toLowerCase()] = d.value;
                        });
                        // compute values for KW, I, V, PF
                        const kw = map['KW'] ?? map['kw'] ?? null;
                        const i = map['I-avg'] ?? map['i-avg'] ?? null;
                        const v = map['V LL Avg'] ?? map['v ll avg'] ?? null;
                        const pf = map['PF'] ?? map['pf'] ?? null;


                        // clear area and draw updated values
                        const ctx2 = _liveRegistry[cid].ctx;
                        ctx2.save();
                        // clear rectangle that covers the four value lines
                        ctx2.fillStyle = 'white';
                        ctx2.fillRect(a.clearLeft - 2, a.clearTop - 2, a.clearWidth + 4, a.clearHeight + 4);

                        // redraw labels (if caller expects labels remain; draw values on top)
                        // draw values
                        ctx2.textAlign = "left";
                        ctx2.textBaseline = "middle";
                        ctx2.font = '600 11px Arial';
                        ctx2.fillStyle = 'red';
                        ctx2.fillText(_fmt(kw), a.x + 0, a.y + 0);   // KW position
                        ctx2.fillText(_fmt(i), a.x + 0, a.y + 15);  // I position
                        ctx2.fillText(_fmt(v), a.x + 0, a.y + 30);  // V position
                        ctx2.fillText(_fmt(pf), a.x + 0, a.y + 45); // PF position
                        ctx2.restore();
                    } catch (err) {
                        // network/parsing errors ignored silently
                        // console.error('live update error', err);
                    }
                }
            }, 1000);
        }
    }

    // helper: draw breaker (moved from original inline script)
    // new optional parameter serDevId: if provided, draws placeholders and registers live update area

    function drawFieldDeviceBreaker(ctx, BreakerImg, title, x, y, fieldDevId) {
        //line
        ctx.beginPath();
        ctx.moveTo(x, y);
        ctx.lineTo(x, y + 25);
        ctx.strokeStyle = "black";
        ctx.lineWidth = 2;
        ctx.stroke();
        //1st circle
        ctx.beginPath();
        ctx.arc(x, y + 27, 2, 0, 2 * Math.PI);
        ctx.strokeStyle = "black";
        ctx.lineWidth = 1;
        ctx.stroke();

        //cross line
        ctx.beginPath();
        ctx.moveTo(x - 10, y + 50);
        ctx.lineTo(x - 2, y + 42);
        ctx.moveTo(x + 2, y + 38);
        ctx.lineTo(x + 10, y + 30);
        ctx.strokeStyle = "black";
        ctx.lineWidth = 2;
        ctx.stroke();

        //2nd circle
        ctx.beginPath();
        ctx.arc(x, y + 40, 2, 0, 2 * Math.PI);
        ctx.strokeStyle = "black";
        ctx.lineWidth = 1;
        ctx.stroke();

        //3rd circle
        ctx.beginPath();
        ctx.arc(x, y + 53, 2, 0, 2 * Math.PI);
        ctx.strokeStyle = "black";
        ctx.lineWidth = 1;
        ctx.stroke();

        //line
        ctx.beginPath();
        ctx.moveTo(x, y + 55);
        ctx.lineTo(x, y + 125);
        ctx.strokeStyle = "black";
        ctx.lineWidth = 2;
        ctx.stroke();

        //breaker image
        if (BreakerImg && BreakerImg.complete) {
            ctx.drawImage(BreakerImg, x + 15, y + 10, 35, 40);
        }

        //KW / I labels
        ctx.textAlign = "center";
        ctx.textBaseline = "middle";
        ctx.font = '600 10px Arial';
        ctx.fillStyle = 'black';
        ctx.fillText("KW", x + 28, y + 63);
        ctx.fillText("I", x + 28, y + 78);
        ctx.fillText("V", x + 28, y + 93);
        ctx.fillText("PF", x + 28, y + 108);

        // values (initial; may be updated by poller if serDevId provided)
        ctx.textAlign = "left";
        ctx.textBaseline = "middle";
        ctx.font = '600 11px Arial';
        ctx.fillStyle = 'red';

        // positions for values relative to x,y
        const valX = x + 42;
        const valY = y + 63;
        // draw placeholders (will be overwritten by poller)
        ctx.fillText("0000", valX, valY);
        ctx.fillText("0000", valX, valY + 15);
        ctx.fillText("0000", valX, valY + 30);
        ctx.fillText("0000", valX, valY + 45);

        // title rendering (unchanged)
        if (title && title.trim().length > 0) {
            const leftX = x + 60;
            const rectWidth = 80;
            const rectHeightSingle = 20;
            const rectHeightDouble = 40;
            const paddingX = 5;
            const fontSpec = '600';
            const lineFontSize = 12;

            ctx.textBaseline = 'top';
            ctx.textAlign = 'left';
            ctx.font = `${fontSpec} ${lineFontSize}px Arial`;

            function splitIntoTwoLines(text, maxWidth) {
                const measured = ctx.measureText(text).width;
                if (measured <= maxWidth) return [text];
                const words = text.split(' ');
                let best = null;
                for (let i = 1; i < words.length; i++) {
                    const left = words.slice(0, i).join(' ');
                    const right = words.slice(i).join(' ');
                    const wLeft = ctx.measureText(left).width;
                    const wRight = ctx.measureText(right).width;
                    const score = Math.max(wLeft, wRight);
                    if (!best || score < best.score) {
                        best = { left, right, wLeft, wRight, score };
                    }
                }
                if (best) return [best.left.trim(), best.right.trim()];
                const idx = Math.floor(text.length / 2);
                return [text.substring(0, idx) + '-', text.substring(idx)];
            }

            const availableTextWidth = rectWidth - paddingX * 2;
            const lines = splitIntoTwoLines(title.trim(), availableTextWidth);

            const isDouble = lines.length === 2;
            const rectH = isDouble ? rectHeightDouble : rectHeightSingle;
            const rectX = leftX - 5;
            const rectY = y + 10;

            if (isDouble) {
                const textY1 = rectY + 5;
                const textY2 = rectY + 25;
                const textWidth1 = ctx.measureText(lines[0]).width;
                const textWidth2 = ctx.measureText(lines[1]).width;
                const rw = Math.max(textWidth1, textWidth2) + 10;
                const ry = textY1 - 5;
                ctx.beginPath();
                ctx.fillStyle = 'white';
                ctx.fillRect(rectX, ry, rw, rectH);
                ctx.strokeStyle = 'black';
                ctx.lineWidth = 0.3;
                ctx.strokeRect(rectX, ry, rw, rectH);

                ctx.fillStyle = 'black';
                ctx.fillText(lines[0], leftX, textY1);
                ctx.fillText(lines[1], leftX, textY2);
            } else {
                const textY = rectY + 15;
                const ry = textY - 5;
                const rw = ctx.measureText(lines[0]).width + 10;
                ctx.beginPath();
                ctx.fillStyle = 'white';
                ctx.fillRect(rectX, ry, rw, rectH);
                ctx.strokeStyle = 'black';
                ctx.lineWidth = 0.3;
                ctx.strokeRect(rectX, ry, rw, rectH);

                ctx.fillStyle = 'black';
                ctx.fillText(lines[0], leftX, textY);
            }
        }

        // if serial device id provided, register area for live updates
        if (fieldDevId) {
            // area specification: where to clear and where to draw values (relative positions)
            const area = {
                // top-left of value block
                x: valX,
                y: valY,
                // clear rectangle covering the four values
                clearLeft: valX - 2,
                clearTop: valY - 8,
                clearWidth: 60,
                clearHeight: 60
            };
            registerFieldDeviceArea(ctx.canvas, ctx, fieldDevId, area);
        }
    }
    function drawBreaker(ctx, BreakerImg, title, x, y, serDevId) {
        //line
        ctx.beginPath();
        ctx.moveTo(x, y);
        ctx.lineTo(x, y + 25);
        ctx.strokeStyle = "black";
        ctx.lineWidth = 2;
        ctx.stroke();
        //1st circle
        ctx.beginPath();
        ctx.arc(x, y + 27, 2, 0, 2 * Math.PI);
        ctx.strokeStyle = "black";
        ctx.lineWidth = 1;
        ctx.stroke();

        //cross line
        ctx.beginPath();
        ctx.moveTo(x - 10, y + 50);
        ctx.lineTo(x - 2, y + 42);
        ctx.moveTo(x + 2, y + 38);
        ctx.lineTo(x + 10, y + 30);
        ctx.strokeStyle = "black";
        ctx.lineWidth = 2;
        ctx.stroke();

        //2nd circle
        ctx.beginPath();
        ctx.arc(x, y + 40, 2, 0, 2 * Math.PI);
        ctx.strokeStyle = "black";
        ctx.lineWidth = 1;
        ctx.stroke();

        //3rd circle
        ctx.beginPath();
        ctx.arc(x, y + 53, 2, 0, 2 * Math.PI);
        ctx.strokeStyle = "black";
        ctx.lineWidth = 1;
        ctx.stroke();

        //line
        ctx.beginPath();
        ctx.moveTo(x, y + 55);
        ctx.lineTo(x, y + 125);
        ctx.strokeStyle = "black";
        ctx.lineWidth = 2;
        ctx.stroke();

        //breaker image
        if (BreakerImg && BreakerImg.complete) {
            ctx.drawImage(BreakerImg, x + 15, y + 10, 35, 40);
        }

        //KW / I labels
        ctx.textAlign = "center";
        ctx.textBaseline = "middle";
        ctx.font = '600 10px Arial';
        ctx.fillStyle = 'black';
        ctx.fillText("KW", x + 28, y + 63);
        ctx.fillText("I", x + 28, y + 78);
        ctx.fillText("V", x + 28, y + 93);
        ctx.fillText("PF", x + 28, y + 108);

        // values (initial; may be updated by poller if serDevId provided)
        ctx.textAlign = "left";
        ctx.textBaseline = "middle";
        ctx.font = '600 11px Arial';
        ctx.fillStyle = 'red';

        // positions for values relative to x,y
        const valX = x + 42;
        const valY = y + 63;
        // draw placeholders (will be overwritten by poller)
        ctx.fillText("0000", valX, valY);
        ctx.fillText("0000", valX, valY + 15);
        ctx.fillText("0000", valX, valY + 30);
        ctx.fillText("0000", valX, valY + 45);

        // title rendering (unchanged)
        if (title && title.trim().length > 0) {
            const leftX = x + 60;
            const rectWidth = 80;
            const rectHeightSingle = 20;
            const rectHeightDouble = 40;
            const paddingX = 5;
            const fontSpec = '600';
            const lineFontSize = 12;

            ctx.textBaseline = 'top';
            ctx.textAlign = 'left';
            ctx.font = `${fontSpec} ${lineFontSize}px Arial`;

            function splitIntoTwoLines(text, maxWidth) {
                const measured = ctx.measureText(text).width;
                if (measured <= maxWidth) return [text];
                const words = text.split(' ');
                let best = null;
                for (let i = 1; i < words.length; i++) {
                    const left = words.slice(0, i).join(' ');
                    const right = words.slice(i).join(' ');
                    const wLeft = ctx.measureText(left).width;
                    const wRight = ctx.measureText(right).width;
                    const score = Math.max(wLeft, wRight);
                    if (!best || score < best.score) {
                        best = { left, right, wLeft, wRight, score };
                    }
                }
                if (best) return [best.left.trim(), best.right.trim()];
                const idx = Math.floor(text.length / 2);
                return [text.substring(0, idx) + '-', text.substring(idx)];
            }

            const availableTextWidth = rectWidth - paddingX * 2;
            const lines = splitIntoTwoLines(title.trim(), availableTextWidth);

            const isDouble = lines.length === 2;
            const rectH = isDouble ? rectHeightDouble : rectHeightSingle;
            const rectX = leftX - 5;
            const rectY = y + 10;

            if (isDouble) {
                const textY1 = rectY + 5;
                const textY2 = rectY + 25;
                const textWidth1 = ctx.measureText(lines[0]).width;
                const textWidth2 = ctx.measureText(lines[1]).width;
                const rw = Math.max(textWidth1, textWidth2) + 10;
                const ry = textY1 - 5;
                ctx.beginPath();
                ctx.fillStyle = 'white';
                ctx.fillRect(rectX, ry, rw, rectH);
                ctx.strokeStyle = 'black';
                ctx.lineWidth = 0.3;
                ctx.strokeRect(rectX, ry, rw, rectH);

                ctx.fillStyle = 'black';
                ctx.fillText(lines[0], leftX, textY1);
                ctx.fillText(lines[1], leftX, textY2);
            } else {
                const textY = rectY + 15;
                const ry = textY - 5;
                const rw = ctx.measureText(lines[0]).width + 10;
                ctx.beginPath();
                ctx.fillStyle = 'white';
                ctx.fillRect(rectX, ry, rw, rectH);
                ctx.strokeStyle = 'black';
                ctx.lineWidth = 0.3;
                ctx.strokeRect(rectX, ry, rw, rectH);

                ctx.fillStyle = 'black';
                ctx.fillText(lines[0], leftX, textY);
            }
        }

        // if serial device id provided, register area for live updates
        if (serDevId) {
            // area specification: where to clear and where to draw values (relative positions)
            const area = {
                // top-left of value block
                x: valX,
                y: valY,
                // clear rectangle covering the four values
                clearLeft: valX - 2,
                clearTop: valY - 8,
                clearWidth: 60,
                clearHeight: 60
            };
            registerSerialDeviceArea(ctx.canvas, ctx, serDevId, area);
        }
    }

    // transformer supports same live-value layout so accept same optional serDevId
    function drawTransformer(ctx, image, title, x, y, serDevId) {
        //line
        ctx.beginPath();
        ctx.moveTo(x, y);
        ctx.lineTo(x, y + 25);
        ctx.strokeStyle = "black";
        ctx.lineWidth = 2;
        ctx.stroke();
        //1st circle
        ctx.beginPath();
        ctx.arc(x, y + 35, 10, 0, 2 * Math.PI);
        ctx.strokeStyle = "black";
        ctx.lineWidth = 1;
        ctx.stroke();

        //2rd circle
        ctx.beginPath();
        ctx.arc(x, y + 45, 10, 0, 2 * Math.PI);
        ctx.strokeStyle = "black";
        ctx.lineWidth = 1;
        ctx.stroke();

        //line
        ctx.beginPath();
        ctx.moveTo(x, y + 55);
        ctx.lineTo(x, y + 125);
        ctx.strokeStyle = "black";
        ctx.lineWidth = 2;
        ctx.stroke();

        //image
        if (image && image.complete) {
            ctx.drawImage(image, x + 15, y + 10, 35, 40);
        }

        //KW / I labels
        ctx.textAlign = "center";
        ctx.textBaseline = "middle";
        ctx.font = '600 10px Arial';
        ctx.fillStyle = 'black';
        ctx.fillText("KW", x + 28, y + 63);
        ctx.fillText("I", x + 28, y + 78);
        ctx.fillText("V", x + 28, y + 93);
        ctx.fillText("PF", x + 28, y + 108);

        // values (initial)
        ctx.textAlign = "left";
        ctx.textBaseline = "middle";
        ctx.font = '600 11px Arial';
        ctx.fillStyle = 'red';

        const valX = x + 42;
        const valY = y + 63;
        ctx.fillText("0000", valX, valY);
        ctx.fillText("0000", valX, valY + 15);
        ctx.fillText("0000", valX, valY + 30);
        ctx.fillText("0000", valX, valY + 45);

        // title (same logic as breaker)
        if (title && title.trim().length > 0) {
            const leftX = x + 60;
            const rectWidth = 80;
            const rectHeightSingle = 20;
            const rectHeightDouble = 40;
            const paddingX = 5;
            const fontSpec = '600';
            const lineFontSize = 12;

            ctx.textBaseline = 'top';
            ctx.textAlign = 'left';
            ctx.font = `${fontSpec} ${lineFontSize}px Arial`;

            function splitIntoTwoLines(text, maxWidth) {
                const measured = ctx.measureText(text).width;
                if (measured <= maxWidth) return [text];
                const words = text.split(' ');
                let best = null;
                for (let i = 1; i < words.length; i++) {
                    const left = words.slice(0, i).join(' ');
                    const right = words.slice(i).join(' ');
                    const wLeft = ctx.measureText(left).width;
                    const wRight = ctx.measureText(right).width;
                    const score = Math.max(wLeft, wRight);
                    if (!best || score < best.score) {
                        best = { left, right, wLeft, wRight, score };
                    }
                }
                if (best) return [best.left.trim(), best.right.trim()];
                const idx = Math.floor(text.length / 2);
                return [text.substring(0, idx) + '-', text.substring(idx)];
            }

            const availableTextWidth = rectWidth - paddingX * 2;
            const lines = splitIntoTwoLines(title.trim(), availableTextWidth);

            const isDouble = lines.length === 2;
            const rectH = isDouble ? rectHeightDouble : rectHeightSingle;
            const rectX = leftX - 5;
            const rectY = y + 10;

            if (isDouble) {
                const textY1 = rectY + 5;
                const textY2 = rectY + 25;
                const textWidth1 = ctx.measureText(lines[0]).width;
                const textWidth2 = ctx.measureText(lines[1]).width;
                const rw = Math.max(textWidth1, textWidth2) + 10;
                const ry = textY1 - 5;
                ctx.beginPath();
                ctx.fillStyle = 'white';
                ctx.fillRect(rectX, ry, rw, rectH);
                ctx.strokeStyle = 'black';
                ctx.lineWidth = 0.3;
                ctx.strokeRect(rectX, ry, rw, rectH);

                ctx.fillStyle = 'black';
                ctx.fillText(lines[0], leftX, textY1);
                ctx.fillText(lines[1], leftX, textY2);
            } else {
                const textY = rectY + 15;
                const ry = textY - 5;
                const rw = ctx.measureText(lines[0]).width + 10;
                ctx.beginPath();
                ctx.fillStyle = 'white';
                ctx.fillRect(rectX, ry, rw, rectH);
                ctx.strokeStyle = 'black';
                ctx.lineWidth = 0.3;
                ctx.strokeRect(rectX, ry, rw, rectH);

                ctx.fillStyle = 'black';
                ctx.fillText(lines[0], leftX, textY);
            }
        }

        // register for live updates if serDevId provided
        if (serDevId) {
            const area = {
                x: valX,
                y: valY,
                clearLeft: valX - 2,
                clearTop: valY - 8,
                clearWidth: 60,
                clearHeight: 60
            };
            registerSerialDeviceArea(ctx.canvas, ctx, serDevId, area);
        }
    }

    function drawOnlyParams(ctx, x, y, serDevId) {

        const valX = x + 43;
        const valY = y + 5;
        //KW / I labels
        ctx.textAlign = "center";
        ctx.textBaseline = "middle";
        ctx.font = '600 10px Arial';
        ctx.fillStyle = 'black';
        ctx.fillText("KW", valX - 14, valY);
        ctx.fillText("I", valX - 14, valY + 15);
        ctx.fillText("V", valX - 14, valY + 30);
        ctx.fillText("PF", valX - 14, valY + 45);

        // values (initial; may be updated by poller if serDevId provided)
        ctx.textAlign = "left";
        ctx.textBaseline = "middle";
        ctx.font = '600 11px Arial';
        ctx.fillStyle = 'red';

        // positions for values relative to x,y

        // draw placeholders (will be overwritten by poller)
        ctx.fillText("0000", valX, valY);
        ctx.fillText("0000", valX, valY + 15);
        ctx.fillText("0000", valX, valY + 30);
        ctx.fillText("0000", valX, valY + 45);

        // if serial device id provided, register area for live updates
        if (serDevId) {
            // area specification: where to clear and where to draw values (relative positions)
            const area = {
                // top-left of value block
                x: valX,
                y: valY,
                // clear rectangle covering the four values
                clearLeft: valX - 2,
                clearTop: valY - 8,
                clearWidth: 60,
                clearHeight: 60
            };
            registerSerialDeviceArea(ctx.canvas, ctx, serDevId, area);
        }
    }
    function drawBusCoupler(ctx, x, y, pos = 'top', pt = false) {
        ctx.beginPath();
        ctx.fillStyle = 'lightgray';
        ctx.fillRect(x - 20, y - 17, 40, 30);
        ctx.rect(x - 20, y - 17, 40, 30);
        ctx.strokeStyle = "black";
        ctx.lineWidth = 1;
        ctx.stroke();

        ctx.textAlign = "center";
        ctx.textBaseline = "middle";
        ctx.font = '400 22px Arial';
        ctx.fillStyle = 'black';
        ctx.fillText("X", x, y);
        ctx.font = '600 12px Arial';
        if (pos === 'top') {
            ctx.fillText("BUS COUPLER", x, y + 25);
            if (pt === true) {
                ctx.fillText("PT", x, y + 40);
            }
        }
        if (pos === 'left') {
            ctx.fillText("BUS COUPLER", x - 65, y);
            if (pt === true) {
                ctx.fillText("PT", x - 65, y + 15);
            }
        }
        if (pos === 'right') {
            ctx.fillText("BUS COUPLER", x + 65, y);
            if (pt === true) {
                ctx.fillText("PT", x - 65, y + 15);
            }
        }

    }

    function drawBusCouplerWithParams(ctx, x, y,serDevId, pos = 'top', pt = false) {
        ctx.beginPath();
        ctx.fillStyle = 'lightgray';
        ctx.fillRect(x - 20, y - 17, 40, 30);
        ctx.rect(x - 20, y - 17, 40, 30);
        ctx.strokeStyle = "black";
        ctx.lineWidth = 1;
        ctx.stroke();

        ctx.textAlign = "center";
        ctx.textBaseline = "middle";
        ctx.font = '400 22px Arial';
        ctx.fillStyle = 'black';
        ctx.fillText("X", x, y);
        ctx.font = '600 12px Arial';
        if (pos === 'top') {
            ctx.fillText("BUS COUPLER", x, y + 25);
            if (pt === true) {
                ctx.fillText("PT", x, y + 40);
            }
            var valX = x - 10;
            var valY = y + 43;
        }

        if (pos === 'left') {
            ctx.fillText("BUS COUPLER", x - 65, y);
            if (pt === true) {
                ctx.fillText("PT", x - 65, y + 15);
            }
        }
        if (pos === 'right') {
            ctx.fillText("BUS COUPLER", x + 65, y);
            if (pt === true) {
                ctx.fillText("PT", x - 65, y + 15);
            }
        }

        //KW / I labels
        ctx.textAlign = "center";
        ctx.textBaseline = "middle";
        ctx.font = '600 10px Arial';
        ctx.fillStyle = 'black';
        ctx.fillText("KW", valX - 14, valY);
        ctx.fillText("I", valX - 14, valY + 15);
        ctx.fillText("V", valX - 14, valY + 30);
        ctx.fillText("PF", valX - 14, valY + 45);

        // values (initial; may be updated by poller if serDevId provided)
        ctx.textAlign = "left";
        ctx.textBaseline = "middle";
        ctx.font = '600 11px Arial';
        ctx.fillStyle = 'red';

        // positions for values relative to x,y

        // draw placeholders (will be overwritten by poller)
        ctx.fillText("0000", valX, valY);
        ctx.fillText("0000", valX, valY + 15);
        ctx.fillText("0000", valX, valY + 30);
        ctx.fillText("0000", valX, valY + 45);

        // if serial device id provided, register area for live updates
        if (serDevId) {
            // area specification: where to clear and where to draw values (relative positions)
            const area = {
                // top-left of value block
                x: valX,
                y: valY,
                // clear rectangle covering the four values
                clearLeft: valX - 2,
                clearTop: valY - 8,
                clearWidth: 60,
                clearHeight: 60
            };
            registerSerialDeviceArea(ctx.canvas, ctx, serDevId, area);
        }
    }

    function drawAdapterPanel(ctx, x, y) {
        ctx.beginPath();
        ctx.fillStyle = 'black';
        ctx.fillRect(x - 20, y - 17, 40, 30);


        ctx.textAlign = "center";
        ctx.textBaseline = "middle";
        ctx.font = '400 22px Arial';
        ctx.fillStyle = 'white';
        ctx.fillText("AP", x, y);
        ctx.font = '600 12px Arial';

    }

    function drawBusPT(ctx, x, y, pos = 'bottom') {
        ctx.beginPath();
        ctx.moveTo(x, y);
        ctx.lineTo(x, y + 110);
        ctx.strokeStyle = "black";
        ctx.lineWidth = 1;
        ctx.stroke();
        ctx.textAlign = "center";
        ctx.textBaseline = "middle";
        ctx.font = '600 12px Arial';
        if (pos === 'bottom') {
            ctx.fillText("BUS PT", x, y + 135);
        }
        if (pos === 'top') {
            ctx.fillText("BUS PT", x, y - 20);
        }
        ctx.beginPath();
        ctx.moveTo(x - 5, y + 110);
        ctx.lineTo(x + 5, y + 110);
        ctx.lineTo(x, y + 125);
        ctx.strokeStyle = 'black';
        ctx.lineWidth = 1;
        ctx.stroke();
        ctx.fillStyle = 'blue';
        ctx.fill();
    }

    // initializer for MPCC fragment
    window.initMpcc = function () {
        const canvas = document.getElementById("mpccCanvas");
        if (!canvas) return;
        const ctx = canvas.getContext("2d");
        const Breaker = document.getElementById("Breaker");
        const Transformer = document.getElementById("Transformer");
        const HtBreaker = document.getElementById("HtBreaker");

        // drawing sequence moved from original inline script
        function drawAll() {
            ctx.clearRect(0, 0, canvas.width, canvas.height);
            var x = 140, y = 20;
            // Example: if you want a breaker to update live using serial device id 123,
            // call drawBreaker(ctx, Breaker, 'CPP1-15MW   52-1', x, y, 123);
            drawBreaker(ctx, HtBreaker, 'CPP1-15MW   52-1', x, y, 281);
            x = 950;
            ctx.fillText("INCOMING FROM TGTRANSCO", x - 100, y - 15);
            drawFieldDeviceBreaker(ctx, HtBreaker, 'GCB MD CONTROLLER', x, y,1);
            drawBreaker(ctx, HtBreaker, 'CPP-2', x + 300, y, 286);

            x = 50; y = 145;
            drawBreaker(ctx, HtBreaker, '52-4', x, y, 284);
            drawBreaker(ctx, HtBreaker, '52-2', x + 150, y, 283);
            drawBreaker(ctx, HtBreaker, 'CPP1 I/C-1     52-3', x + 150, y + 115, 16);

            x = 980;
            drawTransformer(ctx, Transformer, '20 MVA XMER INCOMER', x, y, 3);
            drawBreaker(ctx, HtBreaker, '52-7', x, y + 115, 24);
            drawTransformer(ctx, Transformer, '25MVA XMER INCOMER', x + 240, y, 2);

            x = 140; y = 385;
            drawBreaker(ctx, HtBreaker, 'CM2-I/C', x, y, 18);
            x += 130;
            drawBusPT(ctx, x, y);
            x += 40;
            drawBreaker(ctx, HtBreaker, 'CAPAC', x, y, 7);
            x += 130;
            drawBreaker(ctx, HtBreaker, 'COLONY', x, y, 20);
            x += 140;
            drawBreaker(ctx, HtBreaker, 'CM5-O/G', x, y, 21);
            x += 140;
            drawBreaker(ctx, HtBreaker, 'CHPL & WGL', x, y, 22);
            x += 130;
            drawBusPT(ctx, x, y);
            x += 170;
            //drawBreaker(ctx, Breaker, 'BUS COUPLER', x + 780, y);
            drawBreaker(ctx, HtBreaker, 'CPP-3', x, y, 25);
            x += 140;
            drawBreaker(ctx, HtBreaker, 'RABH-3', x, y, 26);
            x += 140;
            drawBreaker(ctx, HtBreaker, '9MW SOLAR', x, y, 299);


            drawBreaker(ctx, HtBreaker, '52-8', x + 200, y, 8);
            x = 50; y = 515;
            drawBreaker(ctx, HtBreaker, 'CPP1 I/C-2     52-5', x, y, 17);
            //drawBreaker(ctx, Breaker, 'BUS COUPLER', x, y);
            //drawBreaker(ctx, Breaker, 'BUS COUPLER', x + 360, y);

            x = 70; y = 640;

            drawBreaker(ctx, HtBreaker, 'LS-2 CRUSHER', x, y, 15);
            x += 140;
            drawBreaker(ctx, HtBreaker, 'CM1 MD 1800KW', x, y, 14);
            x += 140;
            drawBreaker(ctx, HtBreaker, 'CM1 XMER', x, y, 13);
            x += 140;
            drawBreaker(ctx, HtBreaker, 'UNIT-1 RABH O/G', x, y, 12);
            x += 140;
            drawBreaker(ctx, HtBreaker, 'SPARE', x, y, 11);
            x += 220;
            drawBreaker(ctx, HtBreaker, '52-6      &  PT', x, y, 10);
            x += 140;
            drawBusPT(ctx, x, y);
            //drawBreaker(ctx, Breaker, '52-8', x, y);
            x += 180;
            drawBreaker(ctx, HtBreaker, 'CM 2', x, y, 6);
            x += 140;
            drawBreaker(ctx, HtBreaker, 'RABH 2', x, y, 5);
            x += 140;
            drawBreaker(ctx, HtBreaker, 'LOOP BREAKER', x, y, 4);

            // LINES

            //52-4 to 52-2 and CPP1 I/C-2
            ctx.beginPath();
            x = 50; y = 230;
            ctx.moveTo(x, y);
            ctx.lineTo(x, 515);

            //cpp1 I/C-2 to all
            ctx.moveTo(x, y + 410);
            ctx.lineTo(x + 1400, y + 410);
            ctx.fillText("6.6KV BUS", x + 480, y + 380);


            //52-1 to 52-4 and 52-2
            y = 145;
            ctx.moveTo(x, y);
            ctx.lineTo(x + 150, y);

            //cpp1 ic-1 to all 
            x = 140; y = 385;
            ctx.moveTo(x, y);
            ctx.lineTo(x + 1160, y);
            ctx.fillText("6.6KV BUS", x + 480, y - 30);

            //52-8 to tr-2
            ctx.moveTo(x + 1360, y);
            ctx.lineTo(x + 1360, y - 115);
            ctx.lineTo(x + 1080, y - 115);

            //52-8 to to all
            ctx.moveTo(x + 1360, y + 125);
            ctx.lineTo(x + 1360, y + 180);
            ctx.lineTo(x + 800, y + 180);
            ctx.lineTo(x + 800, y + 255);


            x = 920; y = 145;
            ctx.moveTo(x, y);
            ctx.lineTo(x + 360, y);

            ctx.strokeStyle = "black";
            ctx.lineWidth = 2;
            ctx.stroke();

            drawBusCouplerWithParams(ctx, 900, 385, 23, 'top', false);
            drawBusCoupler(ctx, 1050, 640, pos = 'top', true);
            drawAdapterPanel(ctx, 1130, 640);
            drawAdapterPanel(ctx, 770, 640);
        }

        // ensure images are loaded before drawing
        const imgs = [document.getElementById("Breaker"), document.getElementById("Transformer"), document.getElementById("Dg_motor"), document.getElementById("HtBreaker")];
        let remaining = imgs.filter(i => i).length;
        if (remaining === 0) {
            drawAll();
        } else {
            imgs.forEach(img => {
                if (!img) { remaining--; return; }
                if (img.complete) {
                    remaining--;
                    if (remaining === 0) drawAll();
                } else {
                    img.addEventListener('load', function () { remaining--; if (remaining === 0) drawAll(); });
                }
            });
        }
    };

    window.initU1Rabh = function () {
        const canvas = document.getElementById("u1rabhCanvas");
        if (!canvas) return;
        const ctx = canvas.getContext("2d");
        const Breaker = document.getElementById("Breaker");
        const Transformer = document.getElementById("Transformer");
        const HtBreaker = document.getElementById("HtBreaker");
        ctx.clearRect(0, 0, canvas.width, canvas.height);
        function drawAll() {
            ctx.clearRect(0, 0, canvas.width, canvas.height);
            var x = 900, y = 70;
            ctx.textAlign = "center";
            ctx.textBaseline = "middle";
            ctx.font = '600 12px Arial';
            ctx.fillText("INCOMING FROM MAIN PCC", x, y - 15);
            drawBreaker(ctx, HtBreaker, 'RABH INCOMER-1', x, y, 34);

            x = 110; y = 195;
            drawBreaker(ctx, HtBreaker, 'COAL MILL-1 MAIN MOTOR(400KW)', x, y, 27);
            x += 200;
            drawBreaker(ctx, HtBreaker, 'VRM MAIN MOTOR(900KW)', x, y, 28);
            x += 200;
            drawBreaker(ctx, HtBreaker, 'VRM FAN MOTOR(900KW)', x, y, 29);
            x += 200;
            drawBreaker(ctx, HtBreaker, 'CAPACITOR BANK', x, y, 30);
            x += 200;
            drawBreaker(ctx, HtBreaker, '2MVA TRANSFORMER-1', x, y, 31);
            drawTransformer(ctx, Transformer, 'TRANSFORMER-1 6.6KV/430V', x - 250, y + 150, 38);

            x += 200;
            drawBreaker(ctx, HtBreaker, '2MVA TRANSFORMER-2', x, y, 32);
            drawTransformer(ctx, Transformer, 'TRANSFORMER-2 6.6KV/430V', x + 250, y + 150, 45);
            x += 200;
            drawBreaker(ctx, HtBreaker, 'RABH CONVERTER T/F', x, y, 33);
            x += 200;
            drawBreaker(ctx, HtBreaker, 'PH FAN 1500KW', x, y, 35);


            x = 50; y = 470;
            drawBreaker(ctx, Breaker, 'COAL MILL & COAL CRUSHER MCC', x, y, 36);
            x += 220;
            drawBreaker(ctx, Breaker, 'VRM 1 MCC', x, y, 39);
            x += 180;
            drawBreaker(ctx, Breaker, 'UDB', x, y, 40);
            x += 160;
            drawBreaker(ctx, Breaker, 'ESP MCC', x, y, 41);
            x += 160;
            drawBreaker(ctx, Breaker, 'RBS', x, y, 42);

            x += 250;
            drawBreaker(ctx, Breaker, 'RABH MCC', x, y, 43);
            x += 180;
            drawBreaker(ctx, Breaker, 'KILN FEED MCC', x, y, 44);
            x += 200;
            drawBreaker(ctx, Breaker, 'KILN & CF MCC', x, y, 46);
            x += 160;
            drawBreaker(ctx, Breaker, 'COOLER   MCC', x, y, 47);
            x += 160;
            drawBreaker(ctx, Breaker, 'GA 160 COMP 1&2', x, y, 48);

            // LINES
            x = 110; y = 195;
            ctx.fillText("6.6KV BUS", x + 650, y - 20);
            ctx.beginPath();
            ctx.moveTo(x, y);
            ctx.lineTo(x + 1400, y);

            x = 50; y = 470;
            ctx.fillText("440V BUS", x + 450, y - 20);
            ctx.moveTo(x, y);
            ctx.lineTo(x + 1670, y);

            x = 660; y = 345;
            ctx.moveTo(x, y);
            ctx.lineTo(x + 250, y);
            ctx.lineTo(x + 250, y - 30);

            x = 1360; y = 345;

            ctx.moveTo(x, y);
            ctx.lineTo(x - 250, y);
            ctx.lineTo(x - 250, y - 30);

            ctx.strokeStyle = "black";
            ctx.lineWidth = 2;
            ctx.stroke();

            //bus coupler
            drawBusCoupler(ctx, 910, 470);
        }

        // ensure images are loaded before drawing
        const imgs = [document.getElementById("Breaker"), document.getElementById("Transformer"), document.getElementById("HtBreaker")];
        let remaining = imgs.filter(i => i).length;
        if (remaining === 0) {
            drawAll();
        } else {
            imgs.forEach(img => {
                if (!img) { remaining--; return; }
                if (img.complete) {
                    remaining--;
                    if (remaining === 0) drawAll();
                } else {
                    img.addEventListener('load', function () { remaining--; if (remaining === 0) drawAll(); });
                }
            });
        }
    }

    window.initU2Rabh = function () {
        const canvas = document.getElementById("u2rabhCanvas");
        if (!canvas) return;
        const ctx = canvas.getContext("2d");
        const Breaker = document.getElementById("Breaker");
        const Transformer = document.getElementById("Transformer");
        const HtBreaker = document.getElementById("HtBreaker");
        ctx.clearRect(0, 0, canvas.width, canvas.height);

        function drawAll() {
            ctx.clearRect(0, 0, canvas.width, canvas.height);
            var x = 900, y = 20;
            drawBreaker(ctx, HtBreaker, 'HT INCOMER FROM MPCC', x, y, 67);

            x = 110; y = 145;
            drawTransformer(ctx, Transformer, '2MVA TRANSFORMER-1', x, y, 68);
            drawBreaker(ctx, Breaker, 'LT INCOMER-1', x, y + 125, 81);
            x += 260;
            drawTransformer(ctx, Transformer, '2MVA TRANSFORMER-2', x, y, 69);
            x += 200;
            drawBreaker(ctx, HtBreaker, 'PH FAN', x, y,72);
            x += 160;
            drawBreaker(ctx, HtBreaker, 'CLM', x, y,73);
            x += 160;
            drawBreaker(ctx, HtBreaker, 'VRM-2 MOTOR', x, y,70);
            x += 180;
            drawBreaker(ctx, HtBreaker, 'VRM-2 FAN', x, y,71);
            x += 160;
            drawBreaker(ctx, HtBreaker, 'CAPACITOR BANK', x, y,74);
            x += 180;
            drawBreaker(ctx, HtBreaker, 'RABH FAN', x, y,75);
            x += 160;
            drawBreaker(ctx, HtBreaker, 'KILN MAIN DRIVE', x, y,76);


            x = 50; y = 395;
            drawBreaker(ctx, Breaker, 'COOLER FAN-3', x, y,77);
            x += 140;
            drawBreaker(ctx, Breaker, 'COOLER FAN-4', x, y,78);
            x += 140;
            drawBreaker(ctx, Breaker, 'COOLER FAN-5', x, y,79);
            x += 140;
            drawBreaker(ctx, Breaker, 'MLDB', x, y,80);
            x += 140;
            drawBreaker(ctx, Breaker, 'APFC-1 GA 110 COMP', x, y,82);
            x += 140;
            drawBreaker(ctx, Breaker, 'LS EXT MCC', x, y,84);
            x += 140;
            drawBreaker(ctx, Breaker, 'KILN MCC', x, y,85);
            x += 140;
            drawBreaker(ctx, Breaker, 'VRM', x, y,86);
            x += 140;
            drawBreaker(ctx, Breaker, 'MPDB', x, y,87);
            x += 140;
            drawBreaker(ctx, Breaker, 'BAG HOUSE', x, y,88);
            x += 300;
            drawBreaker(ctx, Breaker, 'LT INCOMER-2', x, y - 80, 93);

            x = 410; y = 560;
            drawBreaker(ctx, Breaker, 'APFC-2', x, y,89);
            x += 120;
            drawBreaker(ctx, Breaker, 'APF PH MCC', x, y,90);
            x += 120;
            drawBreaker(ctx, Breaker, 'BAGHOUSE & COAL FIRING', x, y,91);
            x += 180;
            drawBreaker(ctx, Breaker, 'COAL MILL MCC', x, y,92);
            x += 140;
            drawBreaker(ctx, Breaker, 'COOLER MCC', x, y,94);
            x += 140;
            drawBreaker(ctx, Breaker, 'KF MCC', x, y,95);
            x += 140;
            drawBreaker(ctx, Breaker, 'COOLER FAN-6', x, y,97);
            x += 140;
            drawBreaker(ctx, Breaker, 'HYDRAULIC MCC', x, y,96);


            // LINES
            x = 110; y = 145;
            ctx.fillText("6.6KV BUS", x + 600, y - 20);
            ctx.beginPath();
            ctx.moveTo(x, y);
            ctx.lineTo(x + 1460, y);

            x = 50; y = 395;
            ctx.fillText("440V BUS", x + 700, y - 20);
            ctx.moveTo(x, y);
            ctx.lineTo(x + 1500, y);
            ctx.lineTo(x + 1500, y + 165);
            ctx.lineTo(x + 360, y + 165);

            x = 370; y = 270;
            ctx.moveTo(x, y);
            ctx.lineTo(x, y + 45);
            ctx.lineTo(x + 1240, y + 45);
            ctx.moveTo(x + 1240, y + 170);
            ctx.lineTo(x + 1240, y + 240);
            ctx.lineTo(x + 1180, y + 240);

            //x = 1360; y = 345;
            //ctx.moveTo(x, y);
            //ctx.lineTo(x - 250, y);
            //ctx.lineTo(x - 250, y - 30);

            ctx.strokeStyle = "black";
            ctx.lineWidth = 2;
            ctx.stroke();

            //bus coupler
            drawBusCoupler(ctx, 1550, 470, 'left');
        }

        // ensure images are loaded before drawing
        const imgs = [document.getElementById("Breaker"), document.getElementById("Transformer"), document.getElementById("HtBreaker")];
        let remaining = imgs.filter(i => i).length;
        if (remaining === 0) {
            drawAll();
        } else {
            imgs.forEach(img => {
                if (!img) { remaining--; return; }
                if (img.complete) {
                    remaining--;
                    if (remaining === 0) drawAll();
                } else {
                    img.addEventListener('load', function () { remaining--; if (remaining === 0) drawAll(); });
                }
            });
        }
    }

    window.initU3Rabh = function () {
        const canvas = document.getElementById("u3rabhCanvas");
        if (!canvas) return;
        const ctx = canvas.getContext("2d");
        const Breaker = document.getElementById("Breaker");
        const Transformer = document.getElementById("Transformer");
        const HtBreaker = document.getElementById("HtBreaker");
        ctx.clearRect(0, 0, canvas.width, canvas.height);

        function drawAll() {
            ctx.clearRect(0, 0, canvas.width, canvas.height);
            var x = 900, y = 20;
            drawBreaker(ctx, HtBreaker, 'INCOMER FROM MPCC', x, y,163);

            x = 130; y = 145;


            drawBreaker(ctx, HtBreaker, 'CEMENT_MILL-4 LOAD CENTER', x, y,156);
            x += 200;
            drawTransformer(ctx, Transformer, '2MVA TRANSFORMER-1', x, y,157);
            drawBreaker(ctx, Breaker, 'LT INCOMER-1', x, y + 125,172);
            x += 200;
            drawTransformer(ctx, Transformer, '2MVA TRANSFORMER-2', x, y,158);
            drawBreaker(ctx, Breaker, 'LT INCOMER-2', x, y + 125,183);
            x += 200;
            drawBreaker(ctx, HtBreaker, 'HT_CAPACITOR BANK', x, y,159);
            x += 240;
            drawBreaker(ctx, HtBreaker, 'VRM-3 MOTOR', x, y,160);
            x += 160;
            drawBreaker(ctx, HtBreaker, 'VRM-3 FAN', x, y,161);
            x += 160;
            drawBreaker(ctx, HtBreaker, 'COAL MILL MOTOR', x, y,162);
            x += 150;
            drawBreaker(ctx, HtBreaker, 'PH FAN MOTOR', x, y,164);
            x += 140;
            drawBreaker(ctx, HtBreaker, 'RABH FAN MOTOR', x, y,165);



            x = 800; y = 285;
            drawBreaker(ctx, HtBreaker, 'KILN DRIVE MOTOR', x, y,166);
            x += 180;
            drawBreaker(ctx, HtBreaker, 'COOLER EX FAN MOTOR', x, y,167);
            x += 180;
            drawBreaker(ctx, HtBreaker, 'COAL MILL CA FAN', x, y,168);
            x += 180;
            drawBreaker(ctx, HtBreaker, 'CRUSHER LOAD CENTER', x, y,169);
            x += 180;
            drawBreaker(ctx, HtBreaker, 'HT CAPICITOR BANK', x, y,170);


            x = 210; y = 470;
            drawBreaker(ctx, Breaker, 'LS EXTRACTION', x, y,171);
            x += 180;
            drawBreaker(ctx, Breaker, 'RABH AC UNITS', x, y,300);
            x += 180;
            drawBreaker(ctx, Breaker, 'RABH &    CF', x, y,174);
            x += 180;
            drawBreaker(ctx, Breaker, '443CP1     GA-160', x, y,173);
            x += 180;
            drawBreaker(ctx, Breaker, 'KF & PH    MCC', x, y,176);
            x += 180;
            drawBreaker(ctx, Breaker, 'MPDB', x, y,175);
            x += 150;
            drawBreaker(ctx, Breaker, 'VRM MCC', x, y,178);
            x += 150;
            drawBreaker(ctx, Breaker, 'APFC 1', x, y,177);


            x = 310; y = 620;
            drawBreaker(ctx, Breaker, 'COAL MILL', x, y,179);
            x += 180;
            drawBreaker(ctx, Breaker, 'U-4 EMERGENCY SUPPLY', x, y, 180);
            x += 180;
            drawBreaker(ctx, Breaker, 'KILN MCC', x, y,182);
            x += 180;
            drawBreaker(ctx, Breaker, 'MLDB', x, y,181);
            x += 180;
            drawBreaker(ctx, Breaker, 'COOLER MCC', x, y,185);
            x += 180;
            drawBreaker(ctx, Breaker, 'APFC 2', x, y,184);
            x += 180;
            drawBreaker(ctx, Breaker, '443CP3    GA-160', x, y,186);



            // LINES

            //ht bus
            x = 130; y = 145;
            ctx.fillText("6.6KV BUS", x + 380, y - 20);
            ctx.beginPath();
            ctx.moveTo(x, y);
            ctx.lineTo(x + 1620, y);
            ctx.lineTo(x + 1620, y + 140);
            ctx.lineTo(x + 670, y + 140);


            //lt bus
            x = 210; y = 470;
            ctx.fillText("440V BUS", x + 500, y - 15);
            ctx.moveTo(x, y);
            ctx.lineTo(x + 1380, y);
            ctx.lineTo(x + 1380, y + 150);
            ctx.lineTo(310, y + 150);

            //LT incomer 1 to lt feeders
            x = 330; y = 395;
            ctx.moveTo(x, y);
            ctx.lineTo(x, y + 75);

            // lt incomer 2 to lt feeders
            x = 530; y = 395;
            ctx.moveTo(x, y);
            ctx.lineTo(x, y + 35);
            ctx.lineTo(x + 1200, y + 35);
            ctx.lineTo(x + 1200, y + 180);
            ctx.lineTo(x + 1060, y + 180);

            ctx.strokeStyle = "black";
            ctx.lineWidth = 2;
            ctx.stroke();

            drawBusCoupler(ctx, 1590, 530, 'right');

        }

        // ensure images are loaded before drawing
        const imgs = [document.getElementById("Breaker"), document.getElementById("Transformer"), document.getElementById("HtBreaker")];
        let remaining = imgs.filter(i => i).length;
        if (remaining === 0) {
            drawAll();
        } else {
            imgs.forEach(img => {
                if (!img) { remaining--; return; }
                if (img.complete) {
                    remaining--;
                    if (remaining === 0) drawAll();
                } else {
                    img.addEventListener('load', function () { remaining--; if (remaining === 0) drawAll(); });
                }
            });
        }
    }

    window.initU2Crusher = function () {
        const canvas = document.getElementById("u2crusherCanvas");
        if (!canvas) return;
        const ctx = canvas.getContext("2d");
        const Breaker = document.getElementById("Breaker");
        const Transformer = document.getElementById("Transformer");
        const HtBreaker = document.getElementById("HtBreaker");
        ctx.clearRect(0, 0, canvas.width, canvas.height);

        function drawAll() {
            ctx.clearRect(0, 0, canvas.width, canvas.height);
            var x = 900, y = 50;
            drawBreaker(ctx, HtBreaker, 'INCOMER FROM MPCC', x, y,56);

            x = 630; y = 175;
            drawTransformer(ctx, Transformer, '1250KVA TRANSFORMER', x, y,57);
            drawBreaker(ctx, Breaker, 'LT INCOMER', x, y + 125,59);
            x = 1170;
            drawBreaker(ctx, HtBreaker, 'CRUSHER MAIN MOTOR', x, y,58);


            x = 500; y = 425;
            drawBreaker(ctx, Breaker, 'CRUSHER MCC INCOMER', x, y,60);
            x += 250;
            drawBreaker(ctx, Breaker, 'LT CAPACITOR BANK', x, y,63);
            x += 250;
            drawBreaker(ctx, Breaker, 'MINES WATER PUMP', x, y,61);



            // LINES
            x = 580; y = 175;
            ctx.fillText("6.6KV BUS", x + 200, y - 20);
            ctx.beginPath();
            ctx.moveTo(x, y);
            ctx.lineTo(x + 630, y);

            x = 450; y = 425;
            ctx.fillText("440V BUS", x + 300, y - 20);

            ctx.moveTo(x, y);
            ctx.lineTo(x + 630, y);


            ctx.strokeStyle = "black";
            ctx.lineWidth = 2;
            ctx.stroke();

        }

        // ensure images are loaded before drawing
        const imgs = [document.getElementById("Breaker"), document.getElementById("Transformer"), document.getElementById("HtBreaker")];
        let remaining = imgs.filter(i => i).length;
        if (remaining === 0) {
            drawAll();
        } else {
            imgs.forEach(img => {
                if (!img) { remaining--; return; }
                if (img.complete) {
                    remaining--;
                    if (remaining === 0) drawAll();
                } else {
                    img.addEventListener('load', function () { remaining--; if (remaining === 0) drawAll(); });
                }
            });
        }

    }

    window.initU3Crusher = function () {
        const canvas = document.getElementById("u3crusherCanvas");
        if (!canvas) return;
        const ctx = canvas.getContext("2d");
        const Breaker = document.getElementById("Breaker");
        const Transformer = document.getElementById("Transformer");
        const HtBreaker = document.getElementById("HtBreaker");
        ctx.clearRect(0, 0, canvas.width, canvas.height);

        function drawAll() {
            ctx.clearRect(0, 0, canvas.width, canvas.height);
            var x = 1000, y = 50;
            drawBreaker(ctx, HtBreaker, 'INCOMER FROM RABH-3 PCC', x, y,143);

            x = 580; y = 175;
            drawBreaker(ctx, HtBreaker, 'CRUSHER MOTOR FEEDER 900KW', x, y,144);
            x += 350;
            drawTransformer(ctx, Transformer, '1MVA TRANSFORMER', x, y,145);
            drawBreaker(ctx, Breaker, 'LT INCOMER', x, y + 125,147);
            x += 350;
            drawBreaker(ctx, HtBreaker, 'SPARE', x, y,146);


            x = 400; y = 425;
            drawBreaker(ctx, Breaker, 'RECLAIMER', x, y,148);
            x += 220;
            drawBreaker(ctx, Breaker, 'APFC', x, y,152);
            x += 220;
            drawBreaker(ctx, Breaker, 'MLDB', x, y,150);
            x += 220;
            drawBreaker(ctx, Breaker, 'MINES WATER PUMP MCC', x, y,149);
            x += 220;
            drawBreaker(ctx, Breaker, 'MPDB', x, y,151);
            x += 220;
            drawBreaker(ctx, Breaker, 'SPARE', x, y,);



            // LINES
            x = 500; y = 175;
            ctx.fillText("6.6KV BUS", x + 280, y - 20);
            ctx.beginPath();
            ctx.moveTo(x, y);
            ctx.lineTo(x + 900, y);

            x = 350; y = 425;
            ctx.fillText("440V BUS", x + 280, y - 20);
            ctx.moveTo(x, y);
            ctx.lineTo(x + 1250, y);


            ctx.strokeStyle = "black";
            ctx.lineWidth = 2;
            ctx.stroke();

        }

        // ensure images are loaded before drawing
        const imgs = [document.getElementById("Breaker"), document.getElementById("Transformer"), document.getElementById("HtBreaker")];
        let remaining = imgs.filter(i => i).length;
        if (remaining === 0) {
            drawAll();
        } else {
            imgs.forEach(img => {
                if (!img) { remaining--; return; }
                if (img.complete) {
                    remaining--;
                    if (remaining === 0) drawAll();
                } else {
                    img.addEventListener('load', function () { remaining--; if (remaining === 0) drawAll(); });
                }
            });
        }

    }

    window.initCmPcc2 = function () {
        const canvas = document.getElementById("cmPcc2Canvas");
        if (!canvas) return;
        const ctx = canvas.getContext("2d");
        const Breaker = document.getElementById("Breaker");
        const Transformer = document.getElementById("Transformer");
        const HtBreaker = document.getElementById("HtBreaker");
        ctx.clearRect(0, 0, canvas.width, canvas.height);

        function drawAll() {
            ctx.clearRect(0, 0, canvas.width, canvas.height);
            var x = 850, y = 50;
            ctx.textAlign = "center";
            ctx.textBaseline = "middle";
            ctx.font = '600 12px Arial';
            ctx.fillText("SUPPLY FROM 52-3A, 52-23 MAIN PCC", x, y - 15);
            drawBreaker(ctx, HtBreaker, '6.6KV INCOMER-1', x, y,106);
            x += 500;
            ctx.textAlign = "center";
            ctx.fillText("SUPPLY FROM 52-3A, 52-23 MAIN PCC", x, y - 15);
            drawBreaker(ctx, HtBreaker, '6.6KV INCOMER-2', x, y,109);


            x = 450; y = 175;
            ctx.fillText("SUPPLY FROM MPCC", x - 70, y - 20);
            drawTransformer(ctx, Transformer, 'CM-1 1.5MVA TRANSFORMER', x, y,13);
            drawBreaker(ctx, Breaker, 'LT INCOMER-1', x, y + 125,114);
            x += 250;
            drawBreaker(ctx, HtBreaker, 'CM-1A MAIN MOTOR', x, y,107);
            x += 250;
            drawTransformer(ctx, Transformer, 'CM-2 1.5MVA TRANSFORMER', x, y,108);
            drawBreaker(ctx, Breaker, 'LT INCOMER-2', x, y + 125,121);
            x += 250;
            drawBreaker(ctx, HtBreaker, 'CM-2 MAIN MOTORS', x, y, 110);
            drawOnlyParams(ctx, x, y+125, 111);
            x += 250;
            drawBreaker(ctx, HtBreaker, 'CM-2 O-SEP FAN MOTOR', x, y,112);


            x = 150; y = 425;
            drawBreaker(ctx, Breaker, 'CM-1 MCC-107', x, y,119);
            x += 140;
            drawBreaker(ctx, Breaker, 'PP-1&2 MCC-108&208', x, y,115);
            x += 160;
            drawBreaker(ctx, Breaker, 'U-1&2 CLK,EXT,MCC, APFC & UDB', x, y,116);
            x += 200;
            drawBreaker(ctx, Breaker, 'CM-1A MCC-303', x, y,117);
            x += 240;
            drawBreaker(ctx, Breaker, 'SILO 4&5 MCC, PP-3&4 MCC', x, y,120);
            x += 200;
            drawBreaker(ctx, Breaker, 'U-1&2 PUMP HOUSE MCC', x, y,113);
            x += 200;
            drawBreaker(ctx, Breaker, 'CM-2 MCC-117', x, y,118);


            // LINES
            x = 700; y = 175;
            ctx.fillText("6.6KV BUS", x + 80, y - 20);
            ctx.fillText("6.6KV BUS", x + 580, y - 20);

            ctx.beginPath();
            ctx.moveTo(x, y);
            ctx.lineTo(x + 250, y);

            ctx.moveTo(x + 500, y);
            ctx.lineTo(x + 750, y);

            x = 150; y = 425;
            ctx.fillText("440V BUS", x + 100, y - 20);

            ctx.moveTo(x, y);
            ctx.lineTo(x + 1140, y);

            ctx.strokeStyle = "black";
            ctx.lineWidth = 2;
            ctx.stroke();

            //bus coupler
            drawBusCoupler(ctx, 820, 425);
        }

        // ensure images are loaded before drawing
        const imgs = [document.getElementById("Breaker"), document.getElementById("Transformer"), document.getElementById("HtBreaker")];
        let remaining = imgs.filter(i => i).length;
        if (remaining === 0) {
            drawAll();
        } else {
            imgs.forEach(img => {
                if (!img) { remaining--; return; }
                if (img.complete) {
                    remaining--;
                    if (remaining === 0) drawAll();
                } else {
                    img.addEventListener('load', function () { remaining--; if (remaining === 0) drawAll(); });
                }
            });
        }

    }

    window.initCm45 = function () {
        const canvas = document.getElementById("cm45Canvas");
        if (!canvas) return;
        const ctx = canvas.getContext("2d");
        const Breaker = document.getElementById("Breaker");
        const Transformer = document.getElementById("Transformer");
        const HtBreaker = document.getElementById("HtBreaker");
        ctx.clearRect(0, 0, canvas.width, canvas.height);

        function drawAll() {
            ctx.clearRect(0, 0, canvas.width, canvas.height);
            var x = 550, y = 50;

            drawBreaker(ctx, HtBreaker, 'INCOMER FROM MPCC', x, y,205);
            x = 1350;
            drawBreaker(ctx, HtBreaker, 'INCOMER FROM U-3 RABH', x, y,210);


            x = 150; y = 175;
            drawTransformer(ctx, Transformer, '2MVA TRANSFORMER', x, y,199);
            drawBreaker(ctx, Breaker, 'LT INCOMER-1', x, y + 125,214);
            x += 200;
            drawBreaker(ctx, HtBreaker, 'SEPERATOR', x, y,200);
            x += 160;
            drawBreaker(ctx, HtBreaker, 'CAPCITOR BANK', x, y,201);
            x += 180;
            drawBreaker(ctx, HtBreaker, 'SPARE', x, y,202);
            x += 140;
            drawBreaker(ctx, HtBreaker, 'CEMENT MILL-5 TWIN DRIVES', x, y, 203);
            drawOnlyParams(ctx,  x, y + 125, 204);

            x += 350;
            drawBreaker(ctx, HtBreaker, 'SEPERATOR', x, y,206);
            x += 200;
            drawTransformer(ctx, Transformer, '2MVA TRANSFORMER', x, y,207);
            drawBreaker(ctx, Breaker, 'LT INCOMER-2', x, y + 125,223);
            x += 200;
            drawBreaker(ctx, HtBreaker, 'CEMENT MILL-4 TWIN DRIVES', x, y, 208);
            drawOnlyParams(ctx,  x, y + 125, 209);

            x = 50; y = 425;
            drawBreaker(ctx, Breaker, 'CM-5 MCC', x, y,217);
            x += 140;
            drawBreaker(ctx, Breaker, 'PUMP HOUSE', x, y,218);
            x += 120;
            drawBreaker(ctx, Breaker, 'PACKER 7', x, y,215);
            x += 140;
            drawBreaker(ctx, Breaker, 'MPDB', x, y,212);
            x += 120;
            drawBreaker(ctx, Breaker, 'APFC', x, y,216);
            x += 120;
            drawBreaker(ctx, Breaker, 'GA 160-2', x, y,213);


            x += 260;
            drawBreaker(ctx, Breaker, 'MLDB', x, y,219);
            x += 120;
            drawBreaker(ctx, Breaker, 'APFC', x, y,220);
            x += 120;
            drawBreaker(ctx, Breaker, 'PACKER 5&6', x, y,221);
            x += 140;
            drawBreaker(ctx, Breaker, 'CLINKER EXTRACTION ON MCC309', x, y,222);
            x += 220;
            drawBreaker(ctx, Breaker, 'CM-4 MCC', x, y,224);
            x += 140;
            drawBreaker(ctx, Breaker, 'GA 160-1', x, y,225);



            // LINES
            x = 150; y = 175;
            ctx.fillText("6.6KV BUS", x + 320, y - 20);
            ctx.beginPath();
            ctx.moveTo(x, y);
            ctx.lineTo(x + 680, y);

            x = 1180; y = 175;
            ctx.fillText("6.6KV BUS", x + 90, y - 20);
            ctx.moveTo(x, y);
            ctx.lineTo(x + 400, y);

            x = 50; y = 425;
            ctx.fillText("440V BUS", x + 620, y - 20);
            ctx.moveTo(x, y);
            ctx.lineTo(x + 1640, y);

            ctx.strokeStyle = "black";
            ctx.lineWidth = 2;
            ctx.stroke();

            //bus coupler
            drawBusCoupler(ctx, 870, 425);
        }

        // ensure images are loaded before drawing
        const imgs = [document.getElementById("Breaker"), document.getElementById("Transformer"), document.getElementById("HtBreaker")];
        let remaining = imgs.filter(i => i).length;
        if (remaining === 0) {
            drawAll();
        } else {
            imgs.forEach(img => {
                if (!img) { remaining--; return; }
                if (img.complete) {
                    remaining--;
                    if (remaining === 0) drawAll();
                } else {
                    img.addEventListener('load', function () { remaining--; if (remaining === 0) drawAll(); });
                }
            });
        }

    }

    window.initCoalHandling = function () {
        const canvas = document.getElementById("coalHandlingCanvas");
        if (!canvas) return;
        const ctx = canvas.getContext("2d");
        const Breaker = document.getElementById("Breaker");
        const Transformer = document.getElementById("Transformer");
        const HtBreaker = document.getElementById("HtBreaker");
        ctx.clearRect(0, 0, canvas.width, canvas.height);

        function drawAll() {
            ctx.clearRect(0, 0, canvas.width, canvas.height);


            ctx.rect(450, 40, 800, 265);
            ctx.strokeStyle = "red";
            ctx.lineWidth = 2;
            ctx.stroke();
            ctx.fillStyle = 'red';
            ctx.font = "500 16px Arial";
            ctx.textAlign = "right";
            ctx.textBaseline = 'bottom';
            ctx.fillText("HT PCC", 1250, 40);

            var x = 850, y = 50;
            drawBreaker(ctx, HtBreaker, 'HT INCOMER COAL HANDLING', x, y,261);

            x = 650; y = 175;
            //drawBreaker(ctx, Breaker, 'TR FEEDER', x, y,263);
            drawTransformer(ctx, Transformer, '2MVA TRANSFORMER', x, y ,263);
            drawBreaker(ctx, Breaker, 'LT INCOMER', x, y + 250,264 );
            x += 400;
            drawBreaker(ctx, HtBreaker, 'HT MOTOR', x, y,262);


            ctx.rect(150, 425, 1450, 265);
            ctx.strokeStyle = "red";
            ctx.lineWidth = 2;
            ctx.stroke();
            ctx.fillStyle = 'red';
            ctx.font = "500 16px Arial";
            ctx.textAlign = "right";
            ctx.textBaseline = 'bottom';
            ctx.fillText("LT PCC", 1600, 425);

            x = 350; y = 550;
            drawBreaker(ctx, Breaker, 'MPDB & MLDB', x, y,270);
            x += 180;
            drawBreaker(ctx, Breaker, 'APFC PANEL 400KVAR', x, y,269);
            x += 180;
            drawBreaker(ctx, Breaker, 'RAW COAL HANDLING PH-2 MCC-1', x, y,266);
            x += 220;
            drawBreaker(ctx, Breaker, 'RAW COAL HANDLING PH-2 MCC-2', x, y,267);
            x += 220;
            drawBreaker(ctx, Breaker, 'RAW COAL HANDLING PH-1', x, y,265);
            x += 200;
            drawBreaker(ctx, Breaker, 'ALTERNATE FUEL SYSTEM', x, y,268);


            // LINES
            x = 650; y = 175;
            ctx.fillText("6.6KV BUS", x + 100, y - 20);

            ctx.beginPath();
            ctx.moveTo(x, y);
            ctx.lineTo(x + 400, y);

            x = 650; y = 300;
            ctx.moveTo(x, y);
            ctx.lineTo(x, y + 125);
            x = 350; y = 550;
            ctx.fillText("440V BUS", x + 150, y - 20);

            ctx.moveTo(x, y);
            ctx.lineTo(x + 1000, y);

            ctx.strokeStyle = "black";
            ctx.lineWidth = 2;
            ctx.stroke();

        }

        // ensure images are loaded before drawing
        const imgs = [document.getElementById("Breaker"), document.getElementById("Transformer"), document.getElementById("HtBreaker")];
        let remaining = imgs.filter(i => i).length;
        if (remaining === 0) {
            drawAll();
        } else {
            imgs.forEach(img => {
                if (!img) { remaining--; return; }
                if (img.complete) {
                    remaining--;
                    if (remaining === 0) drawAll();
                } else {
                    img.addEventListener('load', function () { remaining--; if (remaining === 0) drawAll(); });
                }
            });
        }

    }

    window.initWagonLoading = function () {
        const canvas = document.getElementById("wagonLoadingCanvas");
        if (!canvas) return;
        const ctx = canvas.getContext("2d");
        const Breaker = document.getElementById("Breaker");
        const Transformer = document.getElementById("Transformer");
        const HtBreaker = document.getElementById("HtBreaker");
        ctx.clearRect(0, 0, canvas.width, canvas.height);

        function drawAll() {
            ctx.clearRect(0, 0, canvas.width, canvas.height);
            var x = 850, y = 50;
            drawBreaker(ctx, HtBreaker, 'HT INCOMER WAGON LOADING', x, y,244);

            x = 650; y = 175;
            drawTransformer(ctx, Transformer, 'HT TRANSFORMER', x, y,245);
            drawBreaker(ctx, Breaker, 'LT INCOMER', x, y + 125,247);
            x += 400;
            drawBreaker(ctx, HtBreaker, 'TEJA LAND SUPPLY', x, y,246);

            x = 300; y = 425;
            drawBreaker(ctx, Breaker, 'CEMENT LOADING', x, y,249);
            x += 200;
            drawBreaker(ctx, Breaker, 'SWS MCC', x, y,252);
            x += 200;
            drawBreaker(ctx, Breaker, 'CLINKER LOADING', x, y,248);
            x += 200;
            drawBreaker(ctx, Breaker, 'MPDB', x, y,254);
            x += 200;
            drawBreaker(ctx, Breaker, 'PACKER 3A & 3B', x, y,250);
            x += 200;
            drawBreaker(ctx, Breaker, 'WAGON TIPPLER MCC', x, y,251);
            x += 200;
            drawBreaker(ctx, Breaker, 'MLDB & APFC', x, y,253);



            // LINES
            x = 650; y = 175;
            ctx.fillText("6.6KV BUS", x + 100, y - 20);

            ctx.beginPath();
            ctx.moveTo(x, y);
            ctx.lineTo(x + 400, y);


            x = 300; y = 425;
            ctx.fillText("440V BUS", x + 500, y - 20);

            ctx.moveTo(x, y);
            ctx.lineTo(x + 1200, y);

            ctx.strokeStyle = "black";
            ctx.lineWidth = 2;
            ctx.stroke();

        }

        // ensure images are loaded before drawing
        const imgs = [document.getElementById("Breaker"), document.getElementById("Transformer"), document.getElementById("HtBreaker")];
        let remaining = imgs.filter(i => i).length;
        if (remaining === 0) {
            drawAll();
        } else {
            imgs.forEach(img => {
                if (!img) { remaining--; return; }
                if (img.complete) {
                    remaining--;
                    if (remaining === 0) drawAll();
                } else {
                    img.addEventListener('load', function () { remaining--; if (remaining === 0) drawAll(); });
                }
            });
        }


    }

    // Provide a generic initializer invoker if needed
    window.initSLDFragment = function (name) {
        const fn = window['init' + name];
        if (typeof fn === 'function') fn();
    };
})();