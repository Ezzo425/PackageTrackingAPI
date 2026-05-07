const API_BASE_URL = 'http://localhost:5001/api/packages'; // We will change this to your AWS IP later

// Track a specific package
async function trackPackage() {
    const trackingNumber = document.getElementById('trackNumber').value;
    const resultDiv = document.getElementById('trackResult');
    
    if (!trackingNumber) {
        resultDiv.innerHTML = '<div class="alert alert-warning">Please enter a tracking number.</div>';
        return;
    }

    try {
        const response = await fetch(`${API_BASE_URL}/tracking/${trackingNumber}`);
        if (!response.ok) throw new Error('Package not found');
        
        const data = await response.json();
        resultDiv.innerHTML = `
            <div class="alert alert-info">
                <strong>Status:</strong> ${data.status} <br>
                <strong>Current Location:</strong> ${data.location}
            </div>`;
    } catch (error) {
        resultDiv.innerHTML = '<div class="alert alert-danger">Package not found. Please check the number.</div>';
    }
}

// Load all packages for the admin table
async function loadAllPackages() {
    const tableBody = document.getElementById('packagesTableBody');
    tableBody.innerHTML = '<tr><td colspan="4" class="text-center">Loading...</td></tr>';

    try {
        const response = await fetch(API_BASE_URL);
        const data = await response.json();
        
        tableBody.innerHTML = '';
        data.forEach(pkg => {
            const row = `<tr>
                <td>${pkg.id}</td>
                <td><strong>${pkg.trackingNumber}</strong></td>
                <td><span class="badge bg-secondary">${pkg.status}</span></td>
                <td>${pkg.location}</td>
            </tr>`;
            tableBody.innerHTML += row;
        });
    } catch (error) {
        tableBody.innerHTML = '<tr><td colspan="4" class="text-center text-danger">Failed to load packages.</td></tr>';
    }
}

// Load packages on startup
document.addEventListener('DOMContentLoaded', loadAllPackages);
