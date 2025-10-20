// Clear saved dependents when leaving or refreshing
window.addEventListener("beforeunload", () => {
    localStorage.removeItem("expandedDependents");
});

// Restore expanded dependents on load
window.addEventListener("load", () => {
    const saved = JSON.parse(localStorage.getItem("expandedDependents") || "[]");
    saved.forEach(depId => {
        const depDiv = document.getElementById(depId);
        if (depDiv) {
            depDiv.style.display = "block";
            depDiv.style.maxHeight = depDiv.scrollHeight + "px";
            depDiv.style.opacity = "1";
        }
    });
});

// Listen for change events on radios, checkboxes, and dropdowns
document.addEventListener("change", event => {
    const input = event.target;
    if (!input.matches("input[type='radio'], input[type='checkbox'], select")) return;

    let depIdToShow = null;
    let isConditional = false;
    let expanded = JSON.parse(localStorage.getItem("expandedDependents") || "[]");

    if (input.tagName.toLowerCase() === "select") {
        const opt = input.selectedOptions[0];
        if (!opt) return;

        depIdToShow = opt.dataset.dependentid;
        isConditional = opt.dataset.conditional === "true";

        // Hide all dependents for this select EXCEPT the currently selected one
        input.querySelectorAll("option").forEach(o => {
            const depDiv = o.dataset.dependentid && document.getElementById(o.dataset.dependentid);
            if (depDiv && o !== opt) {
                depDiv.style.maxHeight = "0";
                depDiv.style.opacity = "0";
                setTimeout(() => depDiv.style.display = "none", 300);
                expanded = expanded.filter(id => id !== o.dataset.dependentid);
            }
        });
    }
    else if (input.type === "radio") {
        depIdToShow = input.dataset.dependentid;
        isConditional = input.dataset.conditional === "true";

        document.querySelectorAll(`[name='${input.name}']`).forEach(r => {
            const depDiv = r.dataset.dependentid && document.getElementById(r.dataset.dependentid);
            if (depDiv && r !== input) {
                depDiv.style.maxHeight = "0";
                depDiv.style.opacity = "0";
                setTimeout(() => depDiv.style.display = "none", 300);
                expanded = expanded.filter(id => id !== r.dataset.dependentid);
            }
        });
    }
    else if (input.type === "checkbox") {
        depIdToShow = input.dataset.dependentid;
        isConditional = input.dataset.conditional === "true";

        // Check or uncheck toggle for checkbox dependents
        if (depIdToShow) {
            const depDiv = document.getElementById(depIdToShow);
            if (input.checked && isConditional) {
                depDiv.style.display = "block";
                depDiv.style.maxHeight = depDiv.scrollHeight + "px";
                depDiv.style.opacity = "1";
                if (!expanded.includes(depIdToShow)) expanded.push(depIdToShow);
            } else {
                depDiv.style.maxHeight = "0";
                depDiv.style.opacity = "0";
                setTimeout(() => depDiv.style.display = "none", 300);
                expanded = expanded.filter(id => id !== depIdToShow);
            }
        }
    }

    // Show dependent (radio/select) if applicable
    if (isConditional && depIdToShow && (input.type === "radio" || input.tagName.toLowerCase() === "select")) {
        const depDiv = document.getElementById(depIdToShow);
        if (depDiv) {
            depDiv.style.display = "block";
            depDiv.style.maxHeight = depDiv.scrollHeight + "px";
            depDiv.style.opacity = "1";
            if (!expanded.includes(depIdToShow)) expanded.push(depIdToShow);
        }
    }

    localStorage.setItem("expandedDependents", JSON.stringify(expanded));
});
