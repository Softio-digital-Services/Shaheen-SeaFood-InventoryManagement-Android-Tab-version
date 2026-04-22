// ============================================================
//  Garage POS – app.js  (live DB version + Role Management)
// ============================================================

let allProducts = [];
let cart = [];
let currentUser = JSON.parse(localStorage.getItem('pos_user')) || null;

// ── Boot ────────────────────────────────────────────────────
document.addEventListener('DOMContentLoaded', async () => {
    initAuth();
    
    // UI Event Listeners
    document.getElementById('btnLogin').addEventListener('click', handleLogin);
    document.getElementById('btnLogout').addEventListener('click', handleLogout);
    document.getElementById('btnClearCart').addEventListener('click', clearCart);
    document.getElementById('btnCheckout').addEventListener('click', processCheckout);
    
    // Admin Modal listeners
    document.getElementById('btnOpenAddModal').addEventListener('click', () => {
        document.getElementById('addItemModal').classList.remove('hidden');
        document.getElementById('newItemName').focus();
    });
    document.getElementById('btnCloseAddModal').addEventListener('click', () => {
        document.getElementById('addItemModal').classList.add('hidden');
    });
    document.getElementById('btnSubmitItem').addEventListener('click', submitNewItem);

    // Search
    document.getElementById('searchInput').addEventListener('input', (e) => {
        const q = e.target.value.toLowerCase().trim();
        renderProducts(q ? allProducts.filter(p =>
            p.name.toLowerCase().includes(q) ||
            (p.barcode && p.barcode.includes(q)) ||
            (p.sku && p.sku.toLowerCase().includes(q))
        ) : allProducts);
    });

    setupBarcodeScanner();
    setupNotificationSystem();
    
    // Initial Load if logged in
    if(currentUser) {
        await loadProducts();
        startPolling();
    }
});

// ── Authentication ───────────────────────────────────────────
function initAuth() {
    const loginScreen = document.getElementById('loginScreen');
    const container = document.querySelector('.pos-container');
    
    if(!currentUser) {
        loginScreen.classList.remove('hidden');
        container.classList.add('blur');
    } else {
        loginScreen.classList.add('hidden');
        container.classList.remove('blur');
        applyRolePermissions();
    }
}

async function handleLogin() {
    const user = document.getElementById('loginUser').value.trim();
    const pass = document.getElementById('loginPass').value.trim();
    const errEl = document.getElementById('loginError');
    const btn = document.getElementById('btnLogin');

    if(!user || !pass) return;

    btn.disabled = true;
    btn.textContent = 'Authenticating...';
    errEl.classList.add('hidden');

    try {
        const res = await fetch('/api/login', {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify({ username: user, password: pass })
        });

        if(!res.ok) throw new Error("Invalid credentials");

        currentUser = await res.json();
        localStorage.setItem('pos_user', JSON.stringify(currentUser));
        
        initAuth();
        await loadProducts();
        startPolling();
        showToast(`Welcome, ${currentUser.fullName}!`, 'success');
    } catch(err) {
        errEl.classList.remove('hidden');
        errEl.textContent = err.message;
    } finally {
        btn.disabled = false;
        btn.textContent = 'Sign In';
    }
}

function handleLogout() {
    currentUser = null;
    localStorage.removeItem('pos_user');
    location.reload();
}

function applyRolePermissions() {
    const isAdmin = currentUser && currentUser.role === 'Admin';
    document.querySelectorAll('.admin-only').forEach(el => {
        el.classList.toggle('hidden', !isAdmin);
    });
}

// ── Products ────────────────────────────────────────────────
async function loadProducts() {
    try {
        const res = await fetch('/api/products');
        if (!res.ok) throw new Error(`HTTP ${res.status}`);
        allProducts = await res.json();
        renderProducts(allProducts);
        checkLowStockAlerts();
        updateCategoryButtons();
    } catch (err) {
        console.error('Failed to load products:', err);
        document.getElementById('productGrid').innerHTML =
            `<p style="color:#ef4444;padding:20px">⚠️ Connection lost. Retrying...</p>`;
    }
}

function renderProducts(products) {
    const grid = document.getElementById('productGrid');
    grid.innerHTML = '';

    if (products.length === 0) {
        grid.innerHTML = '<p style="color:#6b7280;padding:20px">No items found.</p>';
        return;
    }

    products.forEach(p => {
        const isOut  = !p.isService && p.stock === 0;
        const isLow  = !p.isService && p.stock > 0 && p.stock <= (p.minStock || 5);

        const card = document.createElement('div');
        card.className = 'product-card';
        if (isOut) card.style.opacity = '0.5';

        const icon = p.isService ? '🔧' : getCategoryIcon(p.category);

        card.innerHTML = `
            <div class="product-img">${icon}</div>
            <div class="product-info">
                <h3>${p.name}</h3>
                <p>${p.category}</p>
            </div>
            <div class="product-price">
                <span class="price">$${Number(p.price).toFixed(2)}</span>
                <span class="stock-badge ${isLow || isOut ? 'low' : ''}">
                    ${p.isService ? 'Service' : (isOut ? 'Out of Stock' : (isLow ? `Low: ${p.stock}` : `Stock: ${p.stock}`))}
                </span>
            </div>
        `;

        if (!isOut) card.addEventListener('click', () => addToCart(p));
        grid.appendChild(card);
    });
}

function getCategoryIcon(cat) {
    const map = { Engine: '⚙️', Brakes: '🛑', Suspension: '🔩', Electrical: '⚡', Accessories: '🧰', Services: '🔧' };
    return map[cat] || '📦';
}

async function updateCategoryButtons() {
    try {
        const res = await fetch('/api/categories');
        let cats = ['All Parts'];
        if (res.ok) {
            const apiCats = await res.json();
            cats = [...cats, ...apiCats];
        } else {
            // Fallback to deriving from products if API fails
            cats = ['All Parts', ...new Set(allProducts.map(p => p.category))];
        }

        const container = document.querySelector('.categories');
        if(!container) return;
        container.innerHTML = '';
        cats.forEach((cat, i) => {
            const btn = document.createElement('button');
            btn.className = 'cat-btn' + (i === 0 ? ' active' : '');
            btn.textContent = cat;
            btn.onclick = () => {
                document.querySelectorAll('.cat-btn').forEach(b => b.classList.remove('active'));
                btn.classList.add('active');
                renderProducts(cat === 'All Parts' ? allProducts : allProducts.filter(p => p.category === cat));
            };
            container.appendChild(btn);
        });
    } catch (err) {
        console.error('Failed to update categories:', err);
    }
}

// ── Scanner Logic ───────────────────────────────────────────
let barcodeBuffer = '';
let barcodeTimer  = null;

function setupBarcodeScanner() {
    document.addEventListener('keydown', (e) => {
        // Allow normal input if focused on specific fields
        if (e.target.tagName === 'INPUT' && e.target.id !== 'newItemBarcode') return;
        
        if (barcodeTimer) clearTimeout(barcodeTimer);

        if (e.key === 'Enter') {
            if (barcodeBuffer.length > 3) {
                const modal = document.getElementById('addItemModal');
                if(!modal.classList.contains('hidden')) {
                    document.getElementById('newItemBarcode').value = barcodeBuffer;
                    showToast("Barcode captured!", "info");
                } else {
                    handleBarcodeScan(barcodeBuffer);
                }
            }
            barcodeBuffer = '';
            e.preventDefault();
            return;
        }
        if (e.key.length === 1) barcodeBuffer += e.key;
        barcodeTimer = setTimeout(() => { barcodeBuffer = ''; }, 50);
    });
}

function handleBarcodeScan(barcode) {
    const product = allProducts.find(p => p.barcode === barcode);
    if (product) {
        if (product.stock > 0 || product.isService) addToCart(product);
        else showToast(`⛔ Out of Stock: ${product.name}`, 'error');
    } else {
        showToast(`❓ Barcode unknown: ${barcode}`, 'warn');
    }
}

// ── Admin: Add Item ────────────────────────────────────────
async function submitNewItem() {
    const name = document.getElementById('newItemName').value.trim();
    const cat = document.getElementById('newItemCategory').value;
    const price = parseFloat(document.getElementById('newItemPrice').value);
    const stock = parseInt(document.getElementById('newItemStock').value);
    const barcode = document.getElementById('newItemBarcode').value.trim();

    if(!name || isNaN(price) || isNaN(stock)) {
        showToast("Please fill all required fields correctly.", "warn");
        return;
    }

    const btn = document.getElementById('btnSubmitItem');
    btn.disabled = true;
    btn.textContent = "Saving...";

    try {
        const res = await fetch('/api/add-item', {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify({ name, category: cat, price, stock, barcode })
        });

        if(!res.ok) throw new Error("Failed to add item");

        showToast("✅ Item added to inventory!", "success");
        document.getElementById('addItemModal').classList.add('hidden');
        resetAddItemForm();
        await loadProducts();
    } catch(err) {
        showToast(err.message, "error");
    } finally {
        btn.disabled = false;
        btn.textContent = "Add to Inventory";
    }
}

function resetAddItemForm() {
    document.getElementById('newItemName').value = '';
    document.getElementById('newItemPrice').value = '';
    document.getElementById('newItemStock').value = '0';
    document.getElementById('newItemBarcode').value = '';
}

// ── Cart & Checkout ────────────────────────────────────────
function addToCart(product) {
    const existing = cart.find(i => i.id === product.id);
    if (existing) {
        if (product.isService || existing.qty < product.stock) existing.qty++;
        else showToast("Maximum stock reached", "warn");
    } else {
        cart.push({ ...product, qty: 1 });
    }
    updateCartUI();
}

function updateCartQty(id, delta) {
    const item = cart.find(i => i.id === id);
    if (!item) return;
    item.qty += delta;
    if (item.qty <= 0) cart = cart.filter(i => i.id !== id);
    updateCartUI();
}

function clearCart() { cart = []; updateCartUI(); }

async function processCheckout() {
    if (cart.length === 0) return;
    const btn = document.getElementById('btnCheckout');
    btn.disabled = true;
    btn.textContent = 'PROCESSING…';

    try {
        const res = await fetch('/api/checkout', {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify({ items: cart.map(i => ({ id: i.id, name: i.name, price: i.price, qty: i.qty })) })
        });
        if (!res.ok) throw new Error(await res.text());
        
        const result = await res.json();
        showToast(`✅ Sale Complete! Order #${result.orderId}`, 'success');
        clearCart();
        await loadProducts();
    } catch (err) {
        showToast(`❌ Checkout failed: ${err.message}`, 'error');
    } finally {
        btn.disabled = false;
        btn.textContent = 'PROCESS CHECKOUT';
    }
}

// ── UI State ────────────────────────────────────────────────
function updateCartUI() {
    const container = document.getElementById('cartItems');
    let subtotal = 0;

    if (cart.length === 0) {
        container.innerHTML = '<div class="empty-cart-state">Cart is empty</div>';
    } else {
        container.innerHTML = '';
        cart.forEach(item => {
            const itemTotal = item.price * item.qty;
            subtotal += itemTotal;
            const div = document.createElement('div');
            div.className = 'cart-item';
            div.innerHTML = `
                <div class="item-desc">
                    <h4>${item.name}</h4>
                    <p>$${itemTotal.toFixed(2)}</p>
                </div>
                <div class="item-qty">
                    <button class="qty-btn" onclick="updateCartQty(${item.id}, -1)">-</button>
                    <span>${item.qty}</span>
                    <button class="qty-btn" onclick="updateCartQty(${item.id}, 1)">+</button>
                </div>
            `;
            container.appendChild(div);
        });
    }

    const applyTax = document.getElementById('applyTax').checked;
    const tax = applyTax ? (subtotal * 0.10) : 0;
    const total = subtotal + tax;

    document.getElementById('subTotal').innerText = `$${subtotal.toFixed(2)}`;
    document.getElementById('taxTotal').innerText = `$${tax.toFixed(2)}`;
    document.getElementById('grandTotal').innerText = `$${total.toFixed(2)}`;
}

// ── Notifications & Polling ──────────────────────────────────
function setupNotificationSystem() {
    const btnNotif = document.getElementById('btnNotifications');
    const panel = document.getElementById('notifPanel');
    if(!btnNotif || !panel) return;
    
    btnNotif.onclick = (e) => { e.stopPropagation(); panel.classList.toggle('hidden'); };
    document.onclick = (e) => { if(!panel.contains(e.target)) panel.classList.add('hidden'); };
    
    const taxToggle = document.getElementById('applyTax');
    if (taxToggle) taxToggle.onchange = updateCartUI;
}

function checkLowStockAlerts() {
    const lowItems = allProducts.filter(p => !p.isService && p.stock <= (p.minStock || 5));
    const badge = document.getElementById('outOfStockBadge');
    if (badge) {
        badge.innerText = lowItems.length;
        badge.classList.toggle('hidden', lowItems.length === 0);
    }
}

function startPolling() {
    setInterval(async () => {
        if(!document.getElementById('btnCheckout')) return;
        if(document.getElementById('btnCheckout').disabled) return;
        const dot = document.getElementById('syncDot');
        if(dot) dot.classList.add('syncing');
        await loadProducts();
        if(dot) dot.classList.remove('syncing');
    }, 15000);
}

function showToast(msg, type = 'info') {
    let toast = document.getElementById('posToast');
    if (!toast) {
        toast = document.createElement('div');
        toast.id = 'posToast';
        toast.style.cssText = `position:fixed;bottom:24px;left:50%;transform:translateX(-50%);padding:16px 32px;border-radius:16px;font-weight:700;color:#fff;z-index:9999;transition:opacity .3s;backdrop-filter:blur(10px);`;
        document.body.appendChild(toast);
    }
    const colors = { success: '#10b981', error: '#ef4444', warn: '#f59e0b', info: '#3b82f6' };
    toast.style.background = (colors[type] || colors.info) + 'dd';
    toast.style.opacity = '1';
    toast.textContent = msg;
    setTimeout(() => { toast.style.opacity = '0'; }, 4000);
}
