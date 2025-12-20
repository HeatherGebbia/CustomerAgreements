function setupNotificationToggle() {
    const textRadio = document.getElementById("Text");
    const messageRadio = document.getElementById("Message");

    const subjectInput = document.getElementById("Subject");
    const subjectRow = document.getElementById("subjectRow");

    const sendToInput = document.getElementById("SendTo");
    const sendToRow = document.getElementById("sendToRow");

    const sendFromInput = document.getElementById("SendFrom");
    const sendFromRow = document.getElementById("sendFromRow");

    const sendCCInput = document.getElementById("SendCC");
    const sendCCRow = document.getElementById("sendCCRow");

    const sendBCCInput = document.getElementById("SendBCC");
    const sendBCCRow = document.getElementById("sendBCCRow");

    if (!textRadio || !messageRadio ||
        !subjectRow || !sendToRow || !sendFromRow || !sendCCRow || !sendBCCRow) {
        return;
    }

    function showRows() {
        [subjectRow, sendToRow, sendFromRow, sendCCRow, sendBCCRow].forEach(row => {
            row.classList.remove("d-none");
            row.classList.add("d-flex");
        });

        [subjectInput, sendToInput, sendFromInput, sendCCInput, sendBCCInput].forEach(input => {
            if (input) input.disabled = false;
        });
    }

    function hideRows() {
        [subjectRow, sendToRow, sendFromRow, sendCCRow, sendBCCRow].forEach(row => {
            row.classList.remove("d-flex");
            row.classList.add("d-none");
        });

        [subjectInput, sendToInput, sendFromInput, sendCCInput, sendBCCInput].forEach(input => {
            if (input) input.disabled = true;
        });
    }

    function toggle() {
        if (messageRadio.checked) {
            showRows();
        } else {
            hideRows();
        }
    }

    // Wire up change events
    textRadio.addEventListener("change", toggle);
    messageRadio.addEventListener("change", toggle);

    // Initialize on page load (respects Edit vs Create)
    toggle();
}

document.addEventListener("DOMContentLoaded", setupNotificationToggle);
