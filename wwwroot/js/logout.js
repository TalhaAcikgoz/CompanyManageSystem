document.getElementById('logoutButton').addEventListener('click', async function() {
    try {
        const response = await fetch('/api/account/logout', {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json'
            }
        });

        if (response.ok) {
            alert('Logout successful!');
            window.location.href = 'login.html'; // Logout sonrası yönlendirmek istediğiniz sayfa
        } else {
            const errorData = await response.json();
            alert('Logout failed: ' + errorData.Message);
        }
    } catch (error) {
        console.error('Error:', error);
        alert('An error occurred during logout.');
    }
});
