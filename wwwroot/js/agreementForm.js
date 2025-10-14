// Clear saved dependents when leaving or reloading the page
window.addEventListener("beforeunload", function () {
    localStorage.removeItem("expandedDependents");
});

// Restore all previously expanded dependents on page load
window.addEventListener("load", function () {
    const savedDependents = JSON.parse(localStorage.getItem("expandedDependents") || "[]");

    savedDependents.forEach(depId => {
        const depDiv = document.getElementById(depId);
        const radio = document.querySelector(`[data-dependentid='${depId}']`);
        if (depDiv && radio) {
            radio.checked = true;
            depDiv.style.display = "block";
            depDiv.style.maxHeight = depDiv.scrollHeight + "px";
            depDiv.style.opacity = "1";
        }
    });
});

document.addEventListener("click", function (event) {
    const radio = event.target.closest("input[type='radio']");
    if (!radio) return;

    const questionGroup = radio.name;
    const depIdToShow = radio.dataset.dependentid;

    // Load current list of open dependents
    let expandedDependents = JSON.parse(localStorage.getItem("expandedDependents") || "[]");

    // Hide dependents belonging only to *this* radio group
    document.querySelectorAll(`[name='${questionGroup}']`).forEach(r => {
        const depId = r.getAttribute("data-dependentid");
        const depDiv = depId && document.getElementById(depId);
        if (depDiv && depId !== depIdToShow) {
            depDiv.style.transition = "max-height 0.4s ease, opacity 0.4s ease";
            depDiv.style.overflow = "hidden";
            depDiv.style.maxHeight = "0";
            depDiv.style.opacity = "0";
            setTimeout(() => depDiv.style.display = "none", 400);

            // Remove this one from the persisted list
            expandedDependents = expandedDependents.filter(id => id !== depId);
        }
    });

    // Show the dependent if this option is conditional
    if (radio.dataset.conditional === "true") {
        const depDiv = document.getElementById(depIdToShow);
        if (depDiv) {
            depDiv.style.display = "block";
            depDiv.style.overflow = "hidden";
            depDiv.style.maxHeight = "0";
            depDiv.style.opacity = "0";

            setTimeout(() => {
                depDiv.style.transition = "max-height 0.4s ease, opacity 0.4s ease";
                depDiv.style.maxHeight = depDiv.scrollHeight + "px";
                depDiv.style.opacity = "1";
            }, 10);

            // Track it as open
            if (!expandedDependents.includes(depIdToShow)) {
                expandedDependents.push(depIdToShow);
            }
        }
    } else {
        // Non-conditional radio → collapse and clear any stored for this group
        expandedDependents = expandedDependents.filter(id => {
            const element = document.querySelector(`[data-dependentid='${id}']`);
            return element && element.name !== questionGroup;
        });
    }

    // Save updated list of open dependents
    localStorage.setItem("expandedDependents", JSON.stringify(expandedDependents));
});
