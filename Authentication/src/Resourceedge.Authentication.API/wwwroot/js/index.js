// Function for creating an error message for the input fields
const createErrorMessage = (message = 'Error Message', src) => {
  const errorContainer = document.createElement('span')
  const errorIconImage = document.createElement('img')
  const errorText = document.createElement('p')

  errorContainer.className = 'error-container'
  errorIconImage.src = src
  errorIconImage.alt = 'error-icon'
  errorText.className = 'error-text'
  errorText.textContent = message

  errorContainer.appendChild(errorIconImage)
  errorContainer.appendChild(errorText)

  return errorContainer
}

// Loading state function
const startButtonloadingState = (button, buttonClass) => {
  button.classList = `${buttonClass} loading`
  button.disabled = true
  button.innerHTML = `<div class="lds-ring">
              <div></div>
              <div></div>
              <div></div>
            </div>`
}

const endButtonLoadingState = button => {
  button.classList = 'primary'
  button.innerHTML = `Log in`
  button.disabled = false
}

// Simulating an async call with setTimeout
// Do async call here
//const asyncCall = function () {
    
//}

const failedAsyncCall = function doAsyncCall() {
  return new Promise(reject => {
    setTimeout(() => {
      reject({
        status: 'done',
      })
    }, 3000)
  })
}
