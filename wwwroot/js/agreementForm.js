// Clear saved dependents when leaving or refreshing
window.addEventListener("beforeunload", () => {
    localStorage.removeItem("expandedDependents");
});

// Restore expanded dependents on load
window.addEventListener("load", () => {
    // Detect if we're in Edit mode — look for an AgreementID field that already has a value
    const agreementIdInput = document.querySelector("input[name='Agreement.AgreementID']");
    const isEditMode = agreementIdInput && agreementIdInput.value && agreementIdInput.value !== "0";

    // If we're not editing, clear any saved dependents and exit
    if (!isEditMode) {
        localStorage.removeItem("expandedDependents");
        return;
    }

    const saved = JSON.parse(localStorage.getItem("expandedDependents") || "[]");

    // Only dependents marked data-expanded="true" should auto-expand in Edit mode
    document.querySelectorAll(".dependent-question[data-expanded='true']").forEach(depDiv => {
        if (!saved.includes(depDiv.id)) {
            saved.push(depDiv.id);
        }
    });

    saved.forEach(depId => {
        const depDiv = document.getElementById(depId);
        if (depDiv) {
            depDiv.style.display = "block";
            depDiv.style.maxHeight = depDiv.scrollHeight + "px";
            depDiv.style.opacity = "1";

            // Re-enable inputs when showing dependent and trigger revalidation
            depDiv.querySelectorAll("input, select, textarea").forEach(el => {
                el.disabled = false;
                if (el.hasAttribute("required")) {
                    el.reportValidity(); // force browser to recheck immediately
                }
            });

        }
    });

    // Keep this synced
    localStorage.setItem("expandedDependents", JSON.stringify(saved));
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
                setTimeout(() => {
                    depDiv.style.display = "none";
                    // Disable inputs so browser ignores hidden fields
                    depDiv.querySelectorAll("input, select, textarea").forEach(el => el.disabled = true);
                }, 300);
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
                setTimeout(() => {
                    depDiv.style.display = "none";
                    // Disable inputs in hidden dependent
                    depDiv.querySelectorAll("input, select, textarea").forEach(el => el.disabled = true);
                }, 300);
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
            if (!depDiv) return;
            if (input.checked && isConditional) {
                depDiv.style.display = "block";
                depDiv.style.maxHeight = depDiv.scrollHeight + "px";
                depDiv.style.opacity = "1";

                // Re-enable inputs when showing dependent and trigger revalidation
                depDiv.querySelectorAll("input, select, textarea").forEach(el => {
                    el.disabled = false;
                    if (el.hasAttribute("required")) {
                        el.reportValidity(); // force browser to recheck immediately
                    }
                });

                if (!expanded.includes(depIdToShow)) expanded.push(depIdToShow);
            } else {
                depDiv.style.maxHeight = "0";
                depDiv.style.opacity = "0";
                setTimeout(() => {
                    depDiv.style.display = "none";
                    // Disable inputs when hiding
                    depDiv.querySelectorAll("input, select, textarea").forEach(el => el.disabled = true);
                }, 300);
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

            // Re-enable inputs when showing dependent and trigger revalidation
            depDiv.querySelectorAll("input, select, textarea").forEach(el => {
                el.disabled = false;
                if (el.hasAttribute("required")) {
                    el.reportValidity(); // force browser to recheck immediately
                }
            });

            if (!expanded.includes(depIdToShow)) expanded.push(depIdToShow);
        }
    }

    localStorage.setItem("expandedDependents", JSON.stringify(expanded));
});

window.addEventListener("load", () => {
    document.querySelectorAll("form").forEach(form => {       

        form.addEventListener("submit", (e) => {
            const action = e.submitter?.value; // "Save" or "Submit"

            // Always mark as submitted so "Required" messages appear if needed
            form.classList.add("submitted");

            // --- Only enforce full validation if the user clicked Submit ---
            if (action === "Submit") {
                // 1) Sync ALL dependents before validating
                form.querySelectorAll(".dependent-question").forEach(dep => {
                    const visible =
                        dep.offsetParent !== null &&
                        getComputedStyle(dep).display !== "none" &&
                        getComputedStyle(dep).visibility !== "hidden" &&
                        dep.style.maxHeight !== "0px";

                    dep.querySelectorAll("input, select, textarea").forEach(el => {
                        el.disabled = !visible;
                    });
                });

                // 2) Checkbox groups: enforce "at least one" (not all)
                const checkboxGroups = {};
                form.querySelectorAll("input[type='checkbox'][name]").forEach(cb => {
                    const visible = cb.offsetParent !== null && !cb.disabled;
                    if (!visible) return;
                    (checkboxGroups[cb.name] ||= []).push(cb);
                });

                Object.values(checkboxGroups).forEach(group => {
                    const anyChecked = group.some(cb => cb.checked);
                    group.forEach((cb, idx) => cb.required = !anyChecked && idx === 0);
                });

                // 3) Final validity check (only for Submit)
                if (!form.checkValidity()) {
                    e.preventDefault();
                    form.reportValidity();
                }
            }
            else if (action === "Save") {
                // Disable required attributes except customer info before saving
                form.querySelectorAll("[required]").forEach(el => {
                    if (
                        !el.name.includes("Customer.CompanyName") &&
                        !el.name.includes("Customer.ContactName") &&
                        !el.name.includes("Customer.EmailAddress")
                    ) {
                        el.removeAttribute("required");
                    }
                });
            }
        });

        // Keep showing "Required" hints on any native invalid event
        form.addEventListener("invalid", () => form.classList.add("submitted"), true);
    });
});

// Watch all required fields and hide "Required" message once valid 
window.addEventListener("DOMContentLoaded", () => {
    document.addEventListener("input", handleValidationFeedback);
    document.addEventListener("change", handleValidationFeedback);

    function handleValidationFeedback(event) {
        const el = event.target;
        if (!el.matches("input, textarea, select")) return;

        const container = el.closest(".mb-2");
        if (!container) return;
        const message = container.querySelector(".required-message");

        if (message) {
            if (el.checkValidity()) {
                message.style.display = "none";
            } else if (document.querySelector("form.submitted")) {
                message.style.display = "block";
            } else {
                message.style.display = "none";
            }
        }
    }
});

