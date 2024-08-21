document.addEventListener('DOMContentLoaded', async () => {
    try {
        const response = await fetch('/api/account/getuser', {
            method: 'GET',
            headers: {
                'Content-Type': 'application/json'
            }
        });

        if (!response.ok) {
            throw new Error('Kullanıcı bilgileri alınamadı.');
        }

        const userData = await response.json();
        const username = userData.username;
        const role = userData.role;
        const companyName = userData.companyName;

        document.getElementById('welcomeMessage').textContent = `Welcome, ${username}! , Your role is ${role} , Your company is ${companyName}`;

        // Rol'e göre arayüz gösterimi
        if (role === 'Admin') {
            document.getElementById('adminSection').style.display = 'block';
        } else if (role === 'Manager') {
            document.getElementById('managerSection').style.display = 'block';
        } else if (role === 'Personal') {
            document.getElementById('personelSection').style.display = 'block';
            document.getElementById('companyName').textContent = companyName;
        }
    } catch (error) {
        console.error(error);
    }
});

function logout() {
    fetch('/api/account/logout', {
        method: 'POST'
    }).then(() => {
        window.location.href = '/login.html';
    });
}

function listCompanies() {
    // Admin için şirketleri listeleme fonksiyonu
}

function addPersonel() {
    // Manager için personel ekleme fonksiyonu
}

function listPersonel() {
    // Manager için personel listeleme fonksiyonu
}
