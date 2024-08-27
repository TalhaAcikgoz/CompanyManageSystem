document.getElementById('registerForm').addEventListener('submit', async function(event) {
    event.preventDefault();

    const username = document.getElementById('username').value;
    const email = document.getElementById('email').value;
    const password = document.getElementById('password').value;
    const CompanyName = document.getElementById('CompanyName').value;
    const FirstName = document.getElementById('FirstName').value;
    const LastName = document.getElementById('LastName').value;
    const dob = document.getElementById('dob').value;

    try {
        const response = await fetch('/api/account/create', {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify({
                 username, email, 
                 password, role: 'NotManager', 
                 CompanyName, FirstName, LastName, 
                 dob 
            })
        });

        if (response.ok) {
            alert('Registration successful! You can now login.');
            window.location.href = 'login.html';
        } else {
            const errorData = await response.json();
            alert('Registration failed: ' + errorData.message);
        }
    } catch (error) {
        console.error('Error:', error);
        alert('An error occurred during registration.');
    }
});
