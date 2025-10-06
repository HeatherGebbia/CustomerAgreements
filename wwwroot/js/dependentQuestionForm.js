function setupAnswerTypeToggle() {
    const answerType = document.getElementById("AnswerType");
    const listItemsRow = document.getElementById("listItemsRow");
    const listMessage = document.getElementById("listMessage");

    function showRow() {
        listItemsRow.classList.remove("d-none");
        listItemsRow.classList.add("d-flex");  
    }

    function hideRow() {
        listItemsRow.classList.remove("d-flex");
        listItemsRow.classList.add("d-none");  
    }

    function toggle() {
        if (answerType && listItemsRow) {
            const value = (answerType.value || "").toLowerCase();

            if (value.includes("list")) {
                showRow();

                if (value === "radio button list") {
                    listMessage.textContent = "Max items allowed for a radio button list is 3. If you need more than 3 items, please select Drop Down List.";
                } else {
                    listMessage.textContent = "";
                }
            } else {
                hideRow();
                listMessage.textContent = "";
            }
        }
    }

    if (answerType) {
        answerType.addEventListener("change", toggle);
        toggle(); // run once on page load
    }
}

document.addEventListener("DOMContentLoaded", setupAnswerTypeToggle);

