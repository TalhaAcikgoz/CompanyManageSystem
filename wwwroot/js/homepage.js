let username;
let role;
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
        username = userData.username;
        role = userData.role;
        const companyName = userData.companyName;

        document.getElementById('welcomeMessage').textContent = `Welcome, ${username}! , Your role is ${role} , Your company is ${companyName}`;
        
        // Rol'e göre arayüz gösterimi
        if (role === 'Admin') {
            document.getElementById('adminSection').style.display = 'block';
        } else if (role === 'Manager') {
            document.getElementById('managerSection').style.display = 'block';
            // Yönetici izin taleplerini listeleme
            fetch('http://localhost:5057/api/personal/getallleaves')
            .then(response => response.json())
            .then(data => {
                const leaveListManager = document.getElementById('managerLeaveList');
                leaveListManager.innerHTML = '';
                const usersData = data.$values;
                if (Array.isArray(usersData)) {
                    usersData.forEach(user => {
                        const leaves = user.leaves.$values;
                        if (Array.isArray(leaves)) {
                            leaves.forEach(leave => {
                                const row = document.createElement('tr');
                                row.innerHTML = `
                                    <td>${user.userName}</td>
                                    <td>${new Date(leave.startDate).toLocaleDateString()}</td>
                                    <td>${new Date(leave.endDate).toLocaleDateString()}</td>
                                    <td>${leave.reason}</td>   
                                    <td>${leave.isApproved ? 'Onaylı' : 'Onaylanmamış'}</td>
                                    <td>${leave.maxLeaveDays}</td>
                                    <td>
                                        <button onclick="approveLeave(${leave.id})" ${leave.isApproved ? 'disabled' : ''}>Onayla</button>
                                    </td>
                                    <td>
                                        <button onclick="cancelLeave('${user.userName}', ${leave.id})">İptal Et</button>
                                    </td>
                                    
                                `;
                                leaveListManager.appendChild(row);
                            });
                        }
                    });
                } else {
                    leaveListManager.textContent = 'İzin bulunamadı.';
                }
            })
            .catch

            // Yönetici masraf taleplerini listeleme
            fetch('http://localhost:5057/api/personal/getallcosts')
            .then(response => response.json())
            .then(data => {
                const costList = document.getElementById('managerCostList');
                costList.innerHTML = '';
                console.log("data " + data.$values);
                
                const usersData = data.$values;
                if (Array.isArray(usersData)) {
                    usersData.forEach(user => {
                        const costs = user.costs.$values;
                        if (Array.isArray(costs)) {
                            costs.forEach(cost => {
                                const row = document.createElement('tr');
                                row.innerHTML = `
                                    <td>${user.userName}</td>
                                    <td>${cost.amount}</td>
                                    <td>${cost.reason}</td>
                                    <td>${cost.isApproved ? 'Onaylı' : 'Onaylanmamış'}</td>
                                    <td>
                                        <button onclick="approveCost(${cost.id})" ${cost.isApproved ? 'disabled' : ''}>Onayla</button>
                                    </td>
                                    <td>
                                        <button onclick="deleteCost(${cost.id})">Sil</button>
                                    </td>
                                `;
                                costList.appendChild(row);
                            });
                        }
                            });
                        } else {
                            costList.textContent = 'Masraf bulunamadı.';
                        }
            })
            .catch(error => console.error('Error:', error.message));            
        } else if (role === 'Personal') {
            document.getElementById('personelSection').style.display = 'block';
            document.getElementById('companyName').textContent = companyName;
            document.getElementById('profileButton').onclick = function() {
                location.href = `personelProfile.html?username=${username}`;
            };

            // Personel izin taleplerini listeleme
            fetch(`http://localhost:5057/api/personal/getleaves?username=${username}`)
                .then(response => response.json())
                .then(data => {
                    const leaveList = document.getElementById('leaveList');
                    leaveList.innerHTML = ''; // Mevcut listeyi temizle
                    if (data && data.$values && data.$values.length > 0) {
                        data.$values.forEach(leave => {
                            const listItem = document.createElement('li');
                            console.log("hadi bak: " + leave.isApproved );
                            listItem.textContent = `${new Date(leave.startDate).toLocaleDateString()} - ${new Date(leave.endDate).toLocaleDateString()} (Reason: ${leave.reason}) - kalan izin hakki ${leave.maxLeaveDays} - ${leave.isApproved ? 'Onaylı' : 'Onaylanmamış'}`;
                            console.log("leave " + leave.id + " " + leave.startDate + " " + leave.endDate + " " + leave.reason + " " + leave.isApproved);
                            // İptal butonu ekleyin
                            const cancelButton = document.createElement('button');
                            cancelButton.textContent = 'İptal Et';
                            cancelButton.onclick = function() {
                                cancelLeave(username, leave.id);
                            };
                            listItem.appendChild(cancelButton);

                            leaveList.appendChild(listItem);
                        });
                    } else {
                        leaveList.textContent = 'İzin bulunamadı.';
                    }
                })
                .catch(error => console.error('Error:', error));

            // Masraf taleplerini listeleme
            fetch('http://localhost:5057/api/personal/getcosts?username=' + username)
                .then(response => response.json())
                .then(data => {
                    const costList = document.getElementById('costList');
                    costList.innerHTML = '';

                    const costs = data.$values;
                    if (costs && costs.length > 0) {
                        costs.forEach(cost => {
                            const row = document.createElement('tr');
                            row.innerHTML = `
                                <td>${username}</td>
                                <td>${cost.amount}</td>
                                <td>${cost.reason}</td>
                                <td>${cost.isApproved ? 'Onaylı' : 'Onaylanmamış'}</td>
                    <td>
                        <button onclick="deleteCost(${cost.id})">Sil</button>
                    </td>
                            `;
                            costList.appendChild(row);
                        });
                    } else {
                        costList.textContent = 'Masraf bulunamadı.';
                    }
                })
                .catch(error => console.error('Error:', error.message));

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

function cancelLeave(username, leaveId) {
    console.log("username " + username + " leaveId " + leaveId);
    if (confirm("Bu izni iptal etmek istediğinizden emin misiniz?")) {
        fetch(`http://localhost:5057/api/personal/cancelleave?username=${username}&leaveId=${leaveId}`, {
            method: 'DELETE'
        })
        .then(response => {
            if (response.ok) {
                alert('İzin başarıyla iptal edildi.');
                document.location.reload(); // Sayfayı yenileyerek listeyi güncelle
            } else {
                return response.json().then(errorData => {
                    alert('İzin iptal edilemedi.');
                    throw new Error(errorData.message || 'İzin iptal edilemedi.');
                });
            }
        })
        .catch(error => console.error('Error:', error.message));
    }
}

    document.getElementById('leaveButton').addEventListener('click', function() {
        const startDate = document.getElementById('startDates').value;
        const endDate = document.getElementById('leaveDates').value;
            
        // Tarihleri birleştirip 'dates' değişkenine atıyoruz
        const reason = document.getElementById('reason').value;
        
        const leavePeriod = {
            StartDate: startDate,
            EndDate: endDate,
            Reason: reason,
            isApproved: false
        };
    
        fetch(`http://localhost:5057/api/personal/addleave?username=${username}`, {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json'
            },
            body: JSON.stringify(leavePeriod)
        })
        .then(response => response.json())
        .then(data => {
            alert(data.message);
            document.location.reload();
        })
        .catch(error => console.error('Error:', error));
    });
    

    function approveCost(costId) {
        fetch(`http://localhost:5057/api/personal/approvecost?costId=${costId}`, {
            method: 'PUT'
        })
        .then(response => response.json())
        .then(data => {
            alert(data.message);
            document.location.reload();
        })
        .catch(error => console.error('Error:', error));
    }
    
    function deleteCost(costId) {
        if (confirm("Bu masrafı silmek istediğinizden emin misiniz?")) {
            fetch(`http://localhost:5057/api/personal/deletecost?costId=${costId}`, {
                method: 'DELETE'
            })
            .then(response => response.json())
            .then(data => {
                alert(data.message);
                document.location.reload();
            })
            .catch(error => console.error('Error:', error));
        }
    }

    function addCost(costId) {
        const amount = document.getElementById('costAmount').value;
        const reason = document.getElementById('costReason').value;

        console.log(" amount " + amount + " reason " + reason + " username " + username);
        
        const cost = {
            Amount: amount,
            Reason: reason,
            Username: username,
            isApproved: false
        };
    
        fetch(`http://localhost:5057/api/personal/addcost?cost=${cost}`, {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json'
            },
            body: JSON.stringify(cost)
        })
        .then(response => response.json())
        .then(data => {
            alert(data.message);
            document.location.reload();
        })
        .catch(error => console.error('Error:', error.message));
    }

    function approveLeave(leaveId) {
        console.log("leaveId " + leaveId);
        fetch(`http://localhost:5057/api/personal/approveleave?leaveId=${leaveId}`, {
            method: 'PUT'
        })
        .then(response => response.json())
        .then(data => {
            alert(data.message);
            document.location.reload();
        })
        .catch(error => console.error('Error:', error));
    }



    async function fetchUpcomingBirthdays() {
        try {
            const response = await fetch('http://localhost:5057/api/personal/upcoming-birthdays');
            if (!response.ok) {
                throw new Error(`HTTP error! status: ${response.status}`);
            }
            const data = await response.json();
            return data.$values.map(person => ({
                date: new Date(person.birthDate),
                userName: person.userName,
                daysUntilBirthday: person.daysUntilBirthday
            }));
        } catch (error) {
            console.error("Error fetching upcoming birthdays:", error);
            return [];
        }
    }
    
    async function fetchPublicHolidays(year) {
        try {
            const response = await fetch(`https://date.nager.at/api/v3/publicholidays/${year}/TR`);
            if (!response.ok) {
                throw new Error(`HTTP error! status: ${response.status}`);
            }
            const holidays = await response.json();
            console.log("Fetched Holidays:", holidays); // Tatil verilerini kontrol etmek için konsola yazdır
            return holidays.map(holiday => ({
                date: new Date(holiday.date),
                localName: holiday.localName, // Tatil adı
                name: holiday.name // İngilizce tatil adı
            }));
        } catch (error) {
            console.error("Error fetching public holidays:", error);
            return []; // Boş bir dizi döndür
        }
    }
    
    async function createCalendar() {
        const calendarBody = document.getElementById('calendar-body1');
        const managercalendarBody = document.getElementById('calendar-body');
        const currentYear = new Date().getFullYear();
        const currentMonth = new Date().getMonth();
        
        const publicHolidays = await fetchPublicHolidays(currentYear);
        const upcomingBirthdays = await fetchUpcomingBirthdays();
        
        const firstDayOfMonth = new Date(currentYear, currentMonth).getDay() - 1;
        const daysInMonth = new Date(currentYear, currentMonth + 1, 0).getDate();
        
        let date = 1;
        for (let i = 0; i < 6; i++) {
            const row = document.createElement('tr');
            
            for (let j = 0; j < 7; j++) {
                const cell = document.createElement('td');
                if (i === 0 && j < firstDayOfMonth) {
                    const cellText = document.createTextNode('');
                    cell.appendChild(cellText);
                    row.appendChild(cell);
                } else if (date > daysInMonth) {
                    break;
                } else {
                    const cellDate = new Date(currentYear, currentMonth, date);
                    const cellText = document.createTextNode(date);
                    cell.appendChild(cellText);

                    if (cellDate.getDay() === 0 || cellDate.getDay() === 6) { // Pazar (0) veya Cumartesi (6)
                        cell.classList.add('weekend');
                    }
                    
                    // Tatil günlerini kontrol et
                    publicHolidays.forEach(holiday => {
                        if (
                            cellDate.getDate() === holiday.date.getDate() &&
                            cellDate.getMonth() === holiday.date.getMonth() &&
                            cellDate.getFullYear() === holiday.date.getFullYear()
                        ) {
                            cell.classList.add('holiday');
                            cell.classList.add('tooltip');
                            const tooltipText = document.createElement('span');
                            tooltipText.classList.add('tooltiptext');
                            tooltipText.textContent = `Tatil: ${holiday.localName} (${holiday.date.toDateString()})`;
                            cell.appendChild(tooltipText);
                        }
                    });
    
                    // Doğum günlerini kontrol et
                    upcomingBirthdays.forEach(birthday => {
                        if (
                            cellDate.getDate() === birthday.date.getDate() &&
                            cellDate.getMonth() === birthday.date.getMonth()
                        ) {
                            cell.classList.add('birthday');
                            cell.classList.add('tooltip');
                            const tooltipText = document.createElement('span');
                            tooltipText.classList.add('tooltiptext');
                            tooltipText.textContent = `Doğum Günü: ${birthday.userName} (${birthday.daysUntilBirthday} gün kaldı)`;
                            cell.appendChild(tooltipText);
                        }
                    });
                    
                    row.appendChild(cell);
                    date++;
                }
            }
            if(role === 'Personal') {
                calendarBody.appendChild(row);
            }
            else if(role === 'Manager')
                managercalendarBody.appendChild(row);
        }
    }
    createCalendar();