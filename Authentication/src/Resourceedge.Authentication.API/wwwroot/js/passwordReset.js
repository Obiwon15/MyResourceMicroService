const resetSubmitButton = document.querySelector('#reset-submit-button')
const newPasswordInputContainer = document.querySelector(
    '.new-password.input-field--container',
)
const newPasswordInput = document.getElementById('employee-password');
const confirmPasswordInput = document.getElementById('confirm-password');

let newPasswordIcon = document.querySelector('.new-password #password-icon');
let confirmPasswordIcon = document.querySelector(
    '.confirm-password #password-icon'
);
const resetForm = document.querySelector('#auth-form');
const errorMessage = document.querySelector('.error-container');

// Reset Submit Function
resetSubmitButton.addEventListener('click', e => {
    e.preventDefault();
    const passwordValue = document.getElementById('employee-password').value;
    const confirmPasswordValue = document.getElementById('confirm-password').value;
    const token = document.getElementById('token').value;
    const email = document.getElementById('email').value;

    const resetPasswordModel = new FormData();
    resetPasswordModel.append("Email", email);
    resetPasswordModel.append("Password", passwordValue);
    resetPasswordModel.append("ConfirmPassword", confirmPasswordValue);
    resetPasswordModel.append("Token", token);

    errorMessage.style.display = 'none';
    newPasswordInput.classList.remove('error');
    confirmPasswordInput.classList.remove('error');

    startButtonloadingState(resetSubmitButton, 'primary');

    if (newPasswordInput.value !== confirmPasswordInput.value) {
        errorMessage.style.display = 'flex';
        newPasswordInput.classList.add('error');
        confirmPasswordInput.classList.add('error');
        endButtonLoadingState(resetSubmitButton, 'primary');
        return;
    } else {
        const options = {
            method: 'POST',
            body: resetPasswordModel
        };
        fetch("/Auth/ResetPassword", options)
            .then(response => response.json())
            .then(response => {
                if (response.success) {
                    endButtonLoadingState(resetSubmitButton, 'primary');
                    resetForm.parentNode.removeChild(resetForm);

                    document.querySelector('.auth-wrapper > p').style.display = 'none';
                    document.querySelector('.reset-success').style.display = 'flex';
                } else {
                    alert(response.message);
                    endButtonLoadingState(resetSubmitButton, 'primary');
                }
            })
            .catch(error => console.log(error));
    };
})

const handleInputChange = () => {
    if (newPasswordInput.value.length >= 6) {
        resetSubmitButton.disabled = false
        return
    }
    resetSubmitButton.disabled = true
}

// Change handlers for input fields

newPasswordInput.addEventListener('input', e => handleInputChange(e))
confirmPasswordInput.addEventListener('input', e => handleInputChange(e))

newPasswordIcon.addEventListener('click', e => {
    newPasswordInput.type =
        newPasswordInput.type === 'password' ? 'text' : 'password'
})

confirmPasswordIcon.addEventListener('click', () => {
    confirmPasswordInput.type =
        confirmPasswordInput.type === 'password' ? 'text' : 'password'
})
