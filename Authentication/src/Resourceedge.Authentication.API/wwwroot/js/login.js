const loginSumbitButton = document.getElementById('login-submit-button')
const employeeDetailsContainer = document.getElementById('employee-details')
let employeeNameDetail = document.querySelector(
  '.employee-contact--details > h3',
)
let employeeEmailDetail = document.querySelector(
  '.employee-contact--details > p',
)
const passwordField = document.querySelector('fieldset.login-password')
const emailField = document.querySelector('fieldset.login-email')
let checkedIcon = document.getElementById('check-icon')

//loginSumbitButton.addEventListener('click', e => handleLoginSubmit(e))

//const handleLoginSubmit = e => {
//  e.preventDefault()
//  if (e.target.textContent.trim() === 'Continue') {
//    startButtonloadingState(loginSumbitButton, 'primary')
//    asyncCall().then(
//      value => {
//        if (value) {
//          endButtonLoadingState(loginSumbitButton, 'primary')
//          document.querySelector('.auth-wrapper > p').style.marginBottom =
//            '20px'
//          employeeDetailsContainer.style.display = 'flex'

//          //Update Employee email in the password page with provided email input value
//          employeeEmailDetail.innerHTML = emailInput.value

//          // Supposed to update employee name after call to api with the provided email input
//          employeeNameDetail.innerHTML = 'Employee Name'

//          // Hide email input and display password input after confirmation of valid email
//          emailField.parentNode.removeChild(emailField)
//          passwordField.style.display = 'block'
//          passwordIcon.style.display = 'flex'
//          console.log('Disabling')
//          loginSumbitButton.disabled = true
//        }
//      },
//      error => console.log(error),
//    )
//  } else {
//    checkedIcon.style.display = 'none'
//    startButtonloadingState(loginSumbitButton, 'primary')
//    passwordInput.value.length > 2
//      ? asyncCall().then(value => {
//          if (value) {
//            window.location = '/index.html'
//          }
//        })
//      : endButtonLoadingState(loginSumbitButton)
//  }
//}

const emailFormats = ['@genesystechhub.com', '@tenece.com', '@partzshop.com', '@privateestateswa.com','@chloeproducts.com']

// Check Icon
const handleInputChange = ({ currentTarget: { type, value } }) => {
    if (type === 'email') {
        value = `@${value.split("@")[1]}`;
        if(emailFormats.includes(value)) {
            checkedIcon.style.display = 'block'
            loginSumbitButton.disabled = false
            return
        }
        checkedIcon.style.display = 'none'
    }

    if (type === 'password' && value.length > 2) {
        loginSumbitButton.disabled = false
        passwordIcon.style.display = 'block'
    return
  }
  loginSumbitButton.disabled = true
}

// Change handlers for input fields
const emailInput = document.getElementById('email-address')
const passwordInput = document.getElementById('employee-password')

if (emailInput) {
    emailInput.addEventListener('input', e => handleInputChange(e))
} else {
    passwordInput.addEventListener('input', e => handleInputChange(e))
}

// Adding an error message
//document
//  .querySelector('.email.input-field--container')
//  .appendChild(
//    createErrorMessage(
//      'Just add a custom error message here.',
//      '../assets/error-icon.png',
//    ),
//  )

// Password Toggler
let passwordIcon = document.getElementById('password-icon')
passwordIcon.addEventListener('click', () => {
  passwordInput.type = passwordInput.type === 'password' ? 'text' : 'password'
})
