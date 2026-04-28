// STATE MANAGEMENT (Now dynamic with fallbacks)
const DEFAULT_PRODUCTS = [
    { id: 1, name: 'Brake Pads - Front', price: 85.00, stock: 12, category: 'Brakes', image: '🛑' },
    { id: 2, name: 'Oil Filter (Premium)', price: 15.50, stock: 4, category: 'Engine', image: '🛢️' },
    { id: 3, name: 'Spark Plug Platinum', price: 8.99, stock: 25, category: 'Engine', image: '⚡' },
    { id: 10, name: 'Full Engine Service', price: 150.00, stock: 999, category: 'Services', image: '🛠️', isService: true }
];

let allProducts = [...DEFAULT_PRODUCTS];
let cart = [];
let currentCategory = 'All';
const API_BASE = ''; 

// CORE INITIALIZATION
document.addEventListener('DOMContentLoaded', async () => {
    // 1. Setup UI Handlers First (Ensures buttons work immediately)
    checkLoginState(); 
    
    // MODAL HANDLERS
    safeListen('btnOpenAddModal', 'click', () => {
        document.getElementById('modalTitle').innerText = 'Add New Product';
        document.getElementById('btnSubmitItem').innerText = 'Add Product';
        document.getElementById('editItemId').value = '';
        document.getElementById('addItemModal').classList.remove('hidden');
    });

    safeListen('btnCloseAddModal', 'click', () => {
        document.getElementById('addItemModal').classList.add('hidden');
    });

    safeListen('btnFullscreen', 'click', () => {
        const enterIcon = document.getElementById('fsIconEnter');
        const exitIcon = document.getElementById('fsIconExit');
        if (!document.fullscreenElement) {
            document.documentElement.requestFullscreen().then(() => {
                enterIcon?.classList.add('hidden');
                exitIcon?.classList.remove('hidden');
            }).catch(() => showToast("Fullscreen restricted", "warn"));
        } else {
            document.exitFullscreen().then(() => {
                enterIcon?.classList.remove('hidden');
                exitIcon?.classList.add('hidden');
            });
        }
    });

    document.addEventListener('fullscreenchange', () => {
        const isFS = !!document.fullscreenElement;
        document.getElementById('fsIconEnter')?.classList.toggle('hidden', isFS);
        document.getElementById('fsIconExit')?.classList.toggle('hidden', !isFS);
    });

    safeListen('btnNotifications', 'click', (e) => {
        e.stopPropagation();
        document.getElementById('notificationPanel').classList.toggle('hidden');
    });

    document.addEventListener('click', () => {
        const panel = document.getElementById('notificationPanel');
        if (panel) panel.classList.add('hidden');
    });

    safeListen('btnCloseScanner', 'click', () => barcodeScannerManager.stop());
    safeListen('btnSubmitItem', 'click', submitNewItem);
    safeListen('btnCameraScan', 'click', () => barcodeScannerManager.start('search'));
    safeListen('btnModalCameraScan', 'click', () => barcodeScannerManager.start('modal'));
    safeListen('btnSwitchCamera', 'click', () => barcodeScannerManager.toggleCamera());
    safeListen('btnClearCart', 'click', clearCart);
    safeListen('btnCheckout', 'click', processCheckout);
    safeListen('btnLogin', 'click', handleLogin);
    safeListen('btnLogout', 'click', handleLogout); 
    
    // PASSWORD TOGGLE
    const toggle = document.getElementById('togglePassword');
    const passIn = document.getElementById('loginPass');
    if (toggle && passIn) {
        toggle.onclick = () => {
            const isShowing = passIn.type === 'text';
            passIn.type = isShowing ? 'password' : 'text';
            document.getElementById('eyeOpen').classList.toggle('hidden', isShowing);
            document.getElementById('eyeClosed').classList.toggle('hidden', !isShowing);
            toggle.classList.toggle('active-green', !isShowing);
        };
    }

    setupNotificationSystem();
    setupSignalR(); 

    // 2. Load Data in Background
    initApp();
});

// UTILITIES
function safeListen(id, event, callback) {
    const el = document.getElementById(id);
    if (el) el.addEventListener(event, callback);
}

async function initApp() {
    try {
        await fetchInventory();
        await fetchCategories();
    } catch (e) {
        console.error("Init failed", e);
    }
    renderProducts();
    updateCartUI();
}

async function fetchInventory() {
    try {
        const res = await fetch(`${API_BASE}/api/products`);
        if (res.ok) {
            allProducts = await res.json();
        } else {
            throw new Error("API responded with error");
        }
    } catch (err) {
        console.error("Fetch inventory failed, using fallbacks", err);
        allProducts = [...DEFAULT_PRODUCTS];
        showToast("Offline Mode: Local data loaded", "warn");
    }
}

async function fetchCategories() {
    try {
        const res = await fetch(`${API_BASE}/api/categories`);
        if (res.ok) {
            const cats = await res.json();
            renderCategories(cats);
        }
    } catch (err) {
        renderCategories(['Engine', 'Services']);
    }
}

function renderCategories(apiCategories = []) {
    const container = document.getElementById('categoryList');
    if (!container) return;
    
    const categories = ['All', ...new Set(['Engine', 'Services', ...apiCategories])];
    container.innerHTML = '';
    
    categories.forEach(cat => {
        const btn = document.createElement('button');
        btn.className = `cat-btn ${currentCategory === cat ? 'active' : ''}`;
        btn.innerText = cat === 'All' ? 'All Parts' : cat;
        btn.onclick = () => {
            currentCategory = cat;
            renderCategories();
            renderProducts();
        };
        container.appendChild(btn);
    });
}

function renderProducts() {
    const grid = document.getElementById('productGrid');
    if (!grid) return;
    grid.innerHTML = '';

    const filtered = currentCategory === 'All' 
        ? allProducts 
        : allProducts.filter(p => p.category === currentCategory);

    filtered.forEach(p => {
        const card = document.createElement('div');
        card.className = 'product-card';
        card.onclick = () => addToCart(p);
        card.innerHTML = `
            <div class="card-edit-btn" onclick="event.stopPropagation(); openEditModal(${p.id})">
                <svg viewBox="0 0 24 24" width="16" height="16" stroke="currentColor" stroke-width="2.5" fill="none"><path d="M11 4H4a2 2 0 0 0-2 2v14a2 2 0 0 0 2 2h14a2 2 0 0 0 2-2v-7"></path><path d="M18.5 2.5a2.121 2.121 0 0 1 3 3L12 15l-4 1 1-4L18.5 2.5z"></path></svg>
            </div>
            <div class="product-img">${p.image || '📦'}</div>
            <div class="product-info">
                <div class="product-name">${p.name}</div>
                <div class="product-price">$${p.price.toFixed(2)}</div>
                <div class="product-stock ${p.stock < 5 ? 'low' : ''}">Stock: ${p.stock}</div>
            </div>
        `;
        grid.appendChild(card);
    });
}

function openEditModal(id) {
    const item = allProducts.find(p => p.id === id);
    if (!item) return;
    document.getElementById('modalTitle').innerText = 'Edit Product';
    document.getElementById('btnSubmitItem').innerText = 'Save Changes';
    document.getElementById('editItemId').value = item.id;
    document.getElementById('newItemName').value = item.name;
    document.getElementById('newItemCategory').value = item.category || 'Engine';
    document.getElementById('newItemPrice').value = item.price;
    document.getElementById('newItemStock').value = item.stock;
    document.getElementById('newItemBarcode').value = item.barcode || '';
    document.getElementById('addItemModal').classList.remove('hidden');
}

async function submitNewItem() {
    const editId = document.getElementById('editItemId').value;
    const itemData = {
        name: document.getElementById('newItemName').value,
        category: document.getElementById('newItemCategory').value,
        price: parseFloat(document.getElementById('newItemPrice').value),
        stock: parseInt(document.getElementById('newItemStock').value),
        barcode: document.getElementById('newItemBarcode').value
    };

    if (!itemData.name || isNaN(itemData.price)) {
        showToast("Please fill Name and Price", "error");
        return;
    }

    try {
        const res = await fetch(`${API_BASE}/api/add-item`, {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify({ ...itemData, id: editId })
        });

        if (res.ok) {
            showToast(editId ? "Item updated!" : "Item added!", "success");
            await initApp(); // Reload everything
            document.getElementById('addItemModal').classList.add('hidden');
        } else {
            const err = await res.json();
            showToast(err.error || "Failed to save", "error");
        }
    } catch (err) {
        showToast("Connection error", "error");
    }
}

function addToCart(product) {
    if (product.stock <= 0) {
        showToast("Out of stock!", "warn");
        return;
    }
    const existing = cart.find(item => item.id === product.id);
    if (existing) {
        existing.quantity++;
    } else {
        cart.push({ ...product, quantity: 1 });
    }
    updateCartUI();
}

function updateCartUI() {
    const list = document.getElementById('cartItems');
    if (!list) return;
    list.innerHTML = '';
    let subtotal = 0;

    cart.forEach(item => {
        const total = item.price * item.quantity;
        subtotal += total;
        const div = document.createElement('div');
        div.className = 'cart-item';
        div.innerHTML = `
            <div class="cart-item-info">
                <div class="cart-item-name">${item.name}</div>
                <div class="cart-item-price">$${item.price.toFixed(2)}</div>
            </div>
            <div class="qty-controls">
                <button class="qty-btn" onclick="changeQty(${item.id}, -1)">-</button>
                <div class="qty-val">${item.quantity}</div>
                <button class="qty-btn" onclick="changeQty(${item.id}, 1)">+</button>
            </div>
            <div class="cart-item-total">$${total.toFixed(2)}</div>
        `;
        list.appendChild(div);
    });

    const taxRate = document.getElementById('applyTax')?.checked ? 0.10 : 0;
    const tax = subtotal * taxRate;
    document.getElementById('subTotal').innerText = `$${subtotal.toFixed(2)}`;
    document.getElementById('taxTotal').innerText = `$${tax.toFixed(2)}`;
    document.getElementById('grandTotal').innerText = `$${(subtotal + tax).toFixed(2)}`;
}

function changeQty(id, delta) {
    const item = cart.find(x => x.id === id);
    if (!item) return;

    if (item.quantity + delta <= 0) {
        cart = cart.filter(x => x.id !== id);
    } else {
        const product = allProducts.find(p => p.id === id);
        if (delta > 0 && product && item.quantity >= product.stock && !product.isService) {
            showToast("No more stock available", "warn");
            return;
        }
        item.quantity += delta;
    }
    updateCartUI();
}

function clearCart() { cart = []; updateCartUI(); }

async function processCheckout() {
    if (cart.length === 0) return;
    
    try {
        const res = await fetch(`${API_BASE}/api/checkout`, {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify({ items: cart.map(i => ({ id: i.id, qty: i.quantity, price: i.price })) })
        });

        if (res.ok) {
            cart = [];
            updateCartUI();
            await fetchInventory();
            renderProducts();
            showToast("Transaction Complete!", "success");
        } else {
            showToast("Checkout failed", "error");
        }
    } catch (err) {
        showToast("Connection error", "error");
    }
}

// LOGIN SYSTEM
function checkLoginState() {
    // ALWAYS clear session on refresh as per user requirement for tablet POS security
    localStorage.removeItem('pos_loggedIn');
    document.getElementById('loginScreen').classList.remove('hidden');
}

async function handleLogin() {
    const user = document.getElementById('loginUser').value;
    const pass = document.getElementById('loginPass').value;

    try {
        const res = await fetch(`${API_BASE}/api/login`, {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify({ username: user, password: pass })
        });

        if (res.ok) {
            const data = await res.json();
            localStorage.setItem('pos_loggedIn', 'true');
            localStorage.setItem('pos_user', data.fullName);
            document.getElementById('loginScreen').classList.add('hidden');
            showToast(`Logged in as ${data.fullName}`, "success");
            initApp();
        } else {
            showToast("Invalid credentials", "error");
        }
    } catch (err) {
        showToast("Login server offline", "error");
    }
}

function handleLogout() {
    localStorage.removeItem('pos_loggedIn');
    document.getElementById('loginScreen').classList.remove('hidden');
    showToast("Logged out", "info");
}

function checkLowStockAlerts() {
    const lowItems = allProducts.filter(p => p.stock <= (p.minStock || 5));
    const badge = document.getElementById('outOfStockBadge');
    const list = document.getElementById('notificationList');
    if (badge) {
        badge.innerText = lowItems.length;
        badge.classList.toggle('hidden', lowItems.length === 0);
    }
    if (list) {
        list.innerHTML = lowItems.length === 0 
            ? '<div class="notif-item"><div class="title">All good!</div></div>' 
            : '';
        lowItems.forEach(item => {
            const div = document.createElement('div');
            const isOut = item.stock <= 0;
            div.className = `notif-item ${isOut ? 'out-of-stock' : 'low-stock'}`;
            div.innerHTML = `<div class="title">${item.name}</div><div class="desc">${isOut ? 'Out of Stock' : 'Low Stock'}: ${item.stock} left</div>`;
            list.appendChild(div);
        });
    }
}

function setupNotificationSystem() {
    const taxToggle = document.getElementById('applyTax');
    if (taxToggle) taxToggle.onchange = updateCartUI;
}

function showToast(msg, type = 'info') {
    const toast = document.createElement('div');
    toast.className = `toast ${type}`;
    toast.style.cssText = `position:fixed; bottom:30px; left:50%; transform:translateX(-50%); padding:12px 24px; background:rgba(20,20,20,0.95); backdrop-filter:blur(10px); border:1px solid var(--border-color); color:white; border-radius:12px; z-index:9000; font-weight:700; box-shadow:0 10px 30px rgba(0,0,0,0.5);`;
    if (type === 'error') toast.style.borderLeft = '4px solid var(--danger)';
    if (type === 'success') toast.style.borderLeft = '4px solid var(--accent)';
    toast.innerText = msg;
    document.body.appendChild(toast);
    setTimeout(() => toast.remove(), 3000);
}

// SIGNALR REAL-TIME SYNC
async function setupSignalR() {
    if (typeof signalR === 'undefined') {
        console.warn("SignalR library not loaded yet.");
        return;
    }

    const connection = new signalR.HubConnectionBuilder()
        .withUrl("/hubs/inventory")
        .withAutomaticReconnect()
        .build();

    connection.on("StockUpdated", (reason) => {
        console.log("Real-time sync: StockUpdated", reason);
        fetchInventory().then(() => renderProducts());
    });

    connection.on("InventoryChanged", (msg) => {
        console.log("Real-time sync: InventoryChanged", msg);
        initApp(); // Full refresh for structural changes
    });

    try {
        await connection.start();
        console.log("SignalR Connected.");
    } catch (err) {
        console.error("SignalR Connection Error", err);
        setTimeout(setupSignalR, 5000);
    }
}

const barcodeScannerManager = {
    scanner: null,
    target: 'search',
    currentFacingMode: "environment",
    async start(target = 'search') {
        this.target = target;
        document.getElementById('cameraScannerModal').classList.remove('hidden');
        if (!this.scanner) this.scanner = new Html5Qrcode("reader");
        try {
            await this.scanner.start({ facingMode: this.currentFacingMode }, { fps: 24, qrbox: (w, h) => { const s = Math.min(w, h) * 0.65; return { width: s, height: s }; } }, (t) => this.onScanSuccess(t), () => {});
        } catch (err) { showToast("Camera error", "error"); }
    },
    async stop() { if (this.scanner) { await this.scanner.stop(); document.getElementById('cameraScannerModal').classList.add('hidden'); } },
    async toggleCamera() { this.currentFacingMode = (this.currentFacingMode === "environment") ? "user" : "environment"; await this.stop(); await this.start(this.target); },
    onScanSuccess(decodedText) {
        if (this.target === 'modal') document.getElementById('newItemBarcode').value = decodedText;
        else {
            const p = allProducts.find(x => x.barcode === decodedText);
            if (p) { addToCart(p); showToast(`Added: ${p.name}`, "success"); }
            else showToast("Unknown barcode", "error");
        }
        this.stop();
    }
};
