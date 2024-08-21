/* document.getElementById('loginForm').addEventListener('submit', async function(event) {
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
            alert('Login successful!');
            window.location.href = 'homepage.html'; // Giriş başarılı olduğunda yönlendirmek istediğin sayfa
        } else {
            const errorData = await response.json();
            alert('Login failed: ' + errorData.Message);
        }
    } catch (error) {
        console.error('Error:', error);
        alert('An error occurred during login.');
    }
});
 */

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

        // Yanıtın JSON formatında olup olmadığını kontrol et
        if (response.ok) {
            // Yanıtı JSON formatında al
            const result = await response.json();
            console.log(result.message);
            alert(result.message); // Giriş başarılı mesajını göster
            window.location.href = 'homepage.html'; // Giriş başarılı olduğunda yönlendirmek istediğin sayfa
        } else {
            // Yanıtı JSON formatında al
            const errorData = await response.json();
            alert('Login failed: ' + errorData); // Hata mesajını göster
        }
    } catch (error) {
        console.error('Error:', error);
        alert('An error occurred during login.');
    }
});
