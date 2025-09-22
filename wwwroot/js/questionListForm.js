function setupConditionalToggle() {
    const yesRadio = document.getElementById("ConditionalYes");
    const noRadio = document.getElementById("ConditionalNo");
    const conditionalRow = document.getElementById("conditionalRow");
    const conditionalMessage = document.getElementById("conditionalMessage");

    function showRow() {
        conditionalRow.classList.remove("d-none");
        conditionalRow.classList.add("d-flex");  
    }

    function hideRow() {
        conditionalRow.classList.remove("d-flex");
        conditionalRow.classList.add("d-none");  
    }

    function toggle() {
        if (yesRadio && yesRadio.checked) showRow(); else hideRow();
    }

    if (yesRadio && noRadio) {
        yesRadio.addEventListener("change", toggle);
        noRadio.addEventListener("change", toggle);
        toggle(); // initialize on page load
    }
}

document.addEventListener("DOMContentLoaded", setupConditionalToggle);

