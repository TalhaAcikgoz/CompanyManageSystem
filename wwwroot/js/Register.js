document.getElementById('registerForm').addEventListener('submit', async function(event) {
    event.preventDefault();

    const username = document.getElementById('username').value;
    const email = document.getElementById('email').value;
    const password = document.getElementById('password').value;
    const CompanyName = document.getElementById('CompanyName').value;

    try {
        const response = await fetch('/api/account/create', {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify({ username, email, password, role: 'Manager', CompanyName }) // 4.rol eklensin onaylaninca manager gelsin
        });

        if (response.ok) {
            alert('Registration successful! You can now login.');
            window.location.href = 'login.html';
        } else {
            const errorData = await response.json();
            alert('Registration failed: ' + errorData.Message);
        }
    } catch (error) {
        console.error('Error:', error);
        alert('An error occurred during registration.');
    }
});
