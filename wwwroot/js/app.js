document.getElementById('createUserForm').addEventListener('submit', async function(event) {
    event.preventDefault();

    const username = document.getElementById('username').value;
    const email = document.getElementById('email').value;
    const password = document.getElementById('password').value;
    const role = document.getElementById('role').value;

    try {
        const response = await fetch('/api/account/create', {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json'
            },
            body: JSON.stringify({
                username: username,
                email: email,
                password: password,
                role: role
            })
        });
        const result = await response.text();
        document.getElementById('result').innerText = result;
    } catch (error) {
        console.error('Error:', error);
        document.getElementById('result').innerText = 'An error occurred while creating the user.';
    }
});
