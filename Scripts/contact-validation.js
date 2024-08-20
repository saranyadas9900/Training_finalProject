// Validation functions for each field
function validateName() {
    const name = document.getElementById('Name').value.trim();
    const nameError = document.getElementById('Name').nextElementSibling;
    if (!name) {
        nameError.innerHTML = "Please enter your name.";
        return false;
    } else {
        nameError.innerHTML = "";
        return true;
    }
}

function validatePhoneNumber() {
    const phone = document.getElementById('PhoneNumber').value.trim();
    const phoneRegex = /^[+]?[0-9]{10,15}$/;
    const phoneError = document.getElementById('PhoneNumber').nextElementSibling;
    if (!phone || !phoneRegex.test(phone)) {
        phoneError.innerHTML = "Please enter a valid phone number.";
        return false;
    } else {
        phoneError.innerHTML = "";
        return true;
    }
}

function validateEmail() {
    const email = document.getElementById('Email').value.trim();
    const emailRegex = /^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$/;
    const emailError = document.getElementById('Email').nextElementSibling;
    if (!email || !emailRegex.test(email)) {
        emailError.innerHTML = "Please enter a valid email address.";
        return false;
    } else {
        emailError.innerHTML = "";
        return true;
    }
}

function validateMessage() {
    const message = document.getElementById('Message').value.trim();
    const messageError = document.getElementById('Message').nextElementSibling;
    if (!message) {
        messageError.innerHTML = "Please enter a message.";
        return false;
    } else {
        messageError.innerHTML = "";
        return true;
    }
}

function validateForm() {
    const isValidName = validateName();
    const isValidPhone = validatePhoneNumber();
    const isValidEmail = validateEmail();
    const isValidMessage = validateMessage();

    return isValidName && isValidPhone && isValidEmail && isValidMessage;
}

// Attach event listeners to form fields
document.addEventListener('DOMContentLoaded', function () {
    document.getElementById('Name').addEventListener('input', validateName);
    document.getElementById('PhoneNumber').addEventListener('input', validatePhoneNumber);
    document.getElementById('Email').addEventListener('input', validateEmail);
    document.getElementById('Message').addEventListener('input', validateMessage);

    // Attach event listener to form submit
    document.getElementById('contactForm').addEventListener('submit', function (event) {
        if (!validateForm()) {
            event.preventDefault(); // Prevent form submission if not valid
        }
    });
});
