function setupInstructionsToggle() {
    const yesRadio = document.getElementById("IncludeYes");
    const noRadio = document.getElementById("IncludeNo");
    const instructionsInput = document.getElementById("Instructions");
    const instructionsRow = document.getElementById("instructionsRow");

    function showRow() {
        instructionsRow.classList.remove("d-none");
        instructionsRow.classList.add("d-flex");   // keep your flex layout
        instructionsInput.disabled = false;
    }

    function hideRow() {
        instructionsRow.classList.remove("d-flex");
        instructionsRow.classList.add("d-none");   // Bootstrap hides with !important
        instructionsInput.disabled = true;
        instructionsInput.value = "";              // optional: clear when hidden
    }

    function toggle() {
        if (yesRadio && yesRadio.checked) showRow(); else hideRow();
    }

    if (yesRadio && noRadio && instructionsInput && instructionsRow) {
        yesRadio.addEventListener("change", toggle);
        noRadio.addEventListener("change", toggle);
        toggle(); // initialize on page load
    }
}

// Ensure jQuery is ready (and validator loaded) before wiring handlers
$(function () {
    setupInstructionsToggle();
});

