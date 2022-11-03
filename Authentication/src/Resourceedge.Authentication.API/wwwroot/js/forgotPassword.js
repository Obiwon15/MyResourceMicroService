const resetSubmitButton = document.getElementById('reset-submit-button')
const emailField = document.getElementById('reset-submit-button')
const resetForm = document.querySelector('#auth-form')

resetSubmitButton.addEventListener('click', e => handleResetSubmit(e))

const handleResetSubmit = e => {
    e.preventDefault();
    debugger;
    startButtonloadingState(resetSubmitButton, 'primary')

    const inputVal = document.getElementById("email-address").value;
    const returnUrl = document.getElementById("returnUrl").value;
    const verifyEmailModel = new FormData();
    verifyEmailModel.append("Email", inputVal);
    verifyEmailModel.append("ReturnUrl", returnUrl);

    const options = {
    method: 'POST',
    body: verifyEmailModel
};
    fetch("/Auth/PasswordReset", options)
        .then(response => {
            return response.json().then(data => ({ data, status: response.status }));
        })
        .then(response => {
            if (response.status === 200) {
            endButtonLoadingState(resetSubmitButton, 'primary');
            resetForm.parentNode.removeChild(resetForm);

            document.querySelector('.auth-wrapper > p').style.display = 'none';
            document.querySelector('.reset-success').style.display = 'flex';
        } else {
            alert("Something went wrong");
        }
    })
    .catch(error => console.log(error));
}


const emailFormats = ['@genesystechhub.com', '@tenece.com', '@partzshop.com', '@privateestateswa.com', '@chloeproducts.com']

// Change handlers
const handleInputChange = ({ currentTarget: { type, value } }) => {

  if (type === 'email') {
     value = `@${value.split("@")[1]}`;
      if (emailFormats.includes(value)) {
          resetSubmitButton.disabled = false
          return
      }
  }

  if (type === 'password' && value.length > 2) {
    resetSubmitButton.disabled = false
    return
  }
  resetSubmitButton.disabled = true
}

// Change handlers for input fields
const _emailInput = document.getElementById('email-address')
const _passwordInput = document.getElementById('employee-password')

_emailInput.addEventListener('input', e => handleInputChange(e))
_passwordInput?.addEventListener('input', e => handleInputChange(e))

// Password Toggler
let _passwordIcon = document.getElementById('password-icon')
_passwordIcon?.addEventListener('click', () => {
  passwordInput.type = passwordInput.type === 'password' ? 'text' : 'password'
})
