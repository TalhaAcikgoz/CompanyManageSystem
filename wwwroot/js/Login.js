document.getElementById('loginForm').addEventListener('submit', async function(event) {
    event.preventDefault();

    const username = document.getElementById('username').value;
    const password = document.getElementById('password').value;

    try {
        const response = await fetch('/api/account/login', {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify({ username, password })
        });

        if (response.ok) {
            const result = await response.json();
            alert(result.message); // Giriş başarılı mesajını göster
            window.location.href = 'homepage.html'; // Giriş başarılı olduğunda yönlendirmek istediğin sayfa
        } else {
            const errorData = await response.json();
            console.log('Error data:', errorData); // Konsola tam hata verisini yazdır
            alert(`Login failed: ${errorData.message || 'Unknown error'}`); // Sadece hata mesajını göster
        }
    } catch (error) {
        console.error('Error:', error.message);
        alert('An error occurred during login.');
    }
});
