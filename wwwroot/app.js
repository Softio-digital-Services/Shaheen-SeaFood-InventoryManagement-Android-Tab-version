// ─── INTERNATIONALIZATION (i18n) ENGINE ───────────────────────────────────
const TRANSLATIONS = {
    en: {
        // Login
        login_title: 'Inventory Portal',
        login_subtitle: 'Sign in to manage your garage',
        login_username: 'Username',
        login_password: 'Password',
        login_btn: 'Sign In',
        // Lock screen
        lock_unlock: 'Unlock',
        lock_switch: 'Switch User / Logout',
        lock_title: 'Session Locked',
        lock_subtitle: 'Enter password to resume',
        // Order / Cart
        order_title: 'Order',
        cart_clear: 'Clear',
        cart_currency: 'Display Currency',
        cart_subtotal: 'Subtotal',
        cart_tax: 'Tax (10%)',
        cart_total: 'Total',
        cart_checkout: 'Process Checkout',
        // Scanner status
        scanner_ready: 'Scanner Ready',
        // Notifications
        notif_header: 'System Alerts',
        notif_all_good: 'All good!',
        notif_low_stock: 'Low Stock',
        notif_out_of_stock: 'Out of Stock',
        // Products
        all_parts: 'All Parts',
        stock_label: 'Stock',
        // Add/Edit Modal
        modal_add_title: 'Add New Product',
        modal_edit_title: 'Edit Product',
        modal_add_btn: 'Add Product',
        modal_save_btn: 'Save Changes',
        modal_field_name: 'Product Name',
        modal_field_name_ph: 'e.g. Brake Pads',
        modal_field_category: 'Category',
        modal_field_price: 'Price ($)',
        modal_field_stock: 'Initial Stock',
        modal_field_barcode: 'Barcode / SKU',
        modal_field_barcode_ph: 'Scan or type barcode',
        // Returns modal
        returns_title: 'Order Returns',
        returns_reason_label: 'Reason for Return',
        returns_reason_ph: 'e.g. Defective item',
        returns_back: 'Back',
        returns_process: 'Process Refund',
        // Camera
        camera_flip: 'Flip Camera',
    },
    ar: {
        // Login
        login_title: 'بوابة المخزون',
        login_subtitle: 'سجّل دخولك لإدارة المستودع',
        login_username: 'اسم المستخدم',
        login_password: 'كلمة المرور',
        login_btn: 'تسجيل الدخول',
        // Lock screen
        lock_unlock: 'إلغاء القفل',
        lock_switch: 'تبديل المستخدم / تسجيل الخروج',
        lock_title: 'الجلسة مقفلة',
        lock_subtitle: 'أدخل كلمة المرور للمتابعة',
        // Order / Cart
        order_title: 'الطلب',
        cart_clear: 'مسح',
        cart_currency: 'عملة العرض',
        cart_subtotal: 'المجموع الفرعي',
        cart_tax: 'الضريبة (10%)',
        cart_total: 'الإجمالي',
        cart_checkout: 'إتمام الدفع',
        // Scanner status
        scanner_ready: 'الماسح جاهز',
        // Notifications
        notif_header: 'تنبيهات النظام',
        notif_all_good: 'كل شيء على ما يرام!',
        notif_low_stock: 'مخزون منخفض',
        notif_out_of_stock: 'نفد المخزون',
        // Products
        all_parts: 'جميع القطع',
        stock_label: 'المخزون',
        // Add/Edit Modal
        modal_add_title: 'إضافة منتج جديد',
        modal_edit_title: 'تعديل المنتج',
        modal_add_btn: 'إضافة المنتج',
        modal_save_btn: 'حفظ التغييرات',
        modal_field_name: 'اسم المنتج',
        modal_field_name_ph: 'مثال: تيل أمامي',
        modal_field_category: 'الفئة',
        modal_field_price: 'السعر',
        modal_field_stock: 'الكمية الابتدائية',
        modal_field_barcode: 'الباركود / الرمز',
        modal_field_barcode_ph: 'امسح أو اكتب الباركود',
        // Returns modal
        returns_title: 'مرتجعات الطلبات',
        returns_reason_label: 'سبب الإرجاع',
        returns_reason_ph: 'مثال: منتج معيب',
        returns_back: 'رجوع',
        returns_process: 'معالجة الاسترداد',
        // Camera
        camera_flip: 'تبديل الكاميرا',
    }
};

let currentLang = localStorage.getItem('pos_lang') || 'en';

function t(key) {
    return (TRANSLATIONS[currentLang] && TRANSLATIONS[currentLang][key]) || TRANSLATIONS['en'][key] || key;
}

function applyLanguage() {
    const isRtl = currentLang === 'ar';
    const html = document.documentElement;

    // Set html attributes
    html.setAttribute('lang', currentLang);
    html.setAttribute('dir', isRtl ? 'rtl' : 'ltr');

    // Update lang toggle button label
    const langLabel = document.getElementById('langLabel');
    if (langLabel) langLabel.textContent = isRtl ? 'EN' : 'AR';

    // Translate all static data-i18n elements
    document.querySelectorAll('[data-i18n]').forEach(el => {
        const key = el.getAttribute('data-i18n');
        el.textContent = t(key);
    });

    // Translate dynamic placeholders
    const searchInput = document.getElementById('searchInput');
    if (searchInput) searchInput.placeholder = isRtl ? 'ابحث في المخزون...' : 'Search inventory...';

    // Add/Edit modal input placeholders
    const newItemName = document.getElementById('newItemName');
    if (newItemName) newItemName.placeholder = t('modal_field_name_ph');

    const newItemBarcode = document.getElementById('newItemBarcode');
    if (newItemBarcode) newItemBarcode.placeholder = t('modal_field_barcode_ph');

    // Returns modal placeholder
    const returnReason = document.getElementById('returnReason');
    if (returnReason) returnReason.placeholder = t('returns_reason_ph');

    // Login screen placeholders
    const loginUser = document.getElementById('loginUser');
    if (loginUser) loginUser.placeholder = isRtl ? 'مدير' : 'admin';

    // Camera modal flip button text
    const btnSwitch = document.getElementById('btnSwitchCamera');
    if (btnSwitch) {
        // Preserve the svg, update the text node
        const textNode = [...btnSwitch.childNodes].find(n => n.nodeType === Node.TEXT_NODE);
        if (textNode) textNode.textContent = '\n                    ' + t('camera_flip') + '\n                ';
    }

    const scannerStatus = document.getElementById('scannerStatus');
    if (scannerStatus) {
        // Preserve the SVG icon, only update the trailing text node
        const textNode = [...scannerStatus.childNodes].find(n => n.nodeType === Node.TEXT_NODE);
        if (textNode) textNode.textContent = '\n                        ' + t('scanner_ready') + '\n                    ';
    }

    // Re-render dynamic content so it picks up new language
    renderCategories();
    renderProducts();
    checkLowStockAlerts();

    // Persist preference
    localStorage.setItem('pos_lang', currentLang);
}

// ─── END i18n ENGINE ──────────────────────────────────────────────────────

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
let currencies = [];
let currentCurrency = { code: 'USD', symbol: '$', rate: 1 };
const API_BASE = ''; 

// CORE INITIALIZATION
document.addEventListener('DOMContentLoaded', async () => {
    // 0. Fetch backend language config immediately to sync web portal language with desktop app natively before UI renders
    await fetchLanguageConfig();
    applyLanguage();

    // 1. Setup UI Handlers First (Ensures buttons work immediately)
    checkLoginState(); 
    


    // MODAL HANDLERS
    safeListen('btnOpenAddModal', 'click', () => {
        document.getElementById('modalTitle').innerText = t('modal_add_title');
        document.getElementById('btnSubmitItem').innerText = t('modal_add_btn');
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
    safeListen('btnLock', 'click', handleLock);
    safeListen('btnUnlock', 'click', handleUnlock);
    safeListen('btnLockLogout', 'click', handleLogout);
    safeListen('btnLogout', 'click', handleLogout); 
    safeListen('btnOpenReturns', 'click', openReturnsModal);
    safeListen('btnCloseReturnsModal', 'click', () => document.getElementById('returnsModal').classList.add('hidden'));
    safeListen('btnProcessReturn', 'click', processReturn);
    
    safeListen('currencySelect', 'change', (e) => {
        const code = e.target.value;
        const curr = currencies.find(c => c.code === code);
        if (curr) {
            currentCurrency = curr;
            renderProducts();
            updateCartUI();
        }
    });    
    safeListen('lockPass', 'keydown', (e) => {
        if (e.key === 'Enter') handleUnlock();
    });    
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
        
        // Login on Enter key
        passIn.addEventListener('keydown', (e) => {
            if (e.key === 'Enter') handleLogin();
        });
        const userIn = document.getElementById('loginUser');
        if (userIn) {
            userIn.addEventListener('keydown', (e) => {
                if (e.key === 'Enter') handleLogin();
            });
        }
    }

    // SEARCH HANDLER
    safeListen('searchInput', 'input', () => {
        renderProducts();
    });

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

async function fetchLanguageConfig() {
    try {
        const res = await fetch(`${API_BASE}/api/config`);
        if (res.ok) {
            const data = await res.json();
            if (data.language && data.language !== currentLang) {
                currentLang = data.language;
                applyLanguage();
            }
            if (data.primaryColor) {
                document.documentElement.style.setProperty('--accent', data.primaryColor);
                document.documentElement.style.setProperty('--accent-hover', data.primaryColor);
            }
            if (data.primaryRgb) {
                document.documentElement.style.setProperty('--accent-rgb', data.primaryRgb);
            }
        }
    } catch (e) { console.error("Language config fetch failed", e); }
}

async function initApp() {
    try {
        await fetchLanguageConfig();
        await fetchInventory();
        await fetchCategories();
        await fetchCurrencies();
        checkLowStockAlerts();
    } catch (e) {
        console.error("Init failed", e);
    }
    renderProducts();
    updateCartUI();
}

async function fetchCurrencies() {
    try {
        const res = await fetch(`${API_BASE}/api/currencies`);
        if (res.ok) {
            currencies = await res.json();
            const select = document.getElementById('currencySelect');
            if (select) {
                select.innerHTML = currencies.map(c => `<option value="${c.code}" ${c.code === currentCurrency.code ? 'selected' : ''}>${c.code} (${c.symbol})</option>`).join('');
            }
        }
    } catch (e) { console.error("Currencies failed", e); }
}

function formatPrice(usdPrice) {
    const converted = usdPrice * currentCurrency.rate;
    return `${currentCurrency.symbol}${converted.toFixed(2)}`;
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
        renderCategories([]);
    }
}

let masterCategories = [];

function renderCategories(apiCategories = null) {
    const container = document.getElementById('categoryList');
    if (!container) return;
    
    // If we received new categories from API, update our master list
    if (apiCategories) {
        masterCategories = apiCategories;
    }

    const categories = ['All', ...masterCategories];
    container.innerHTML = '';
    
    // Sync the Add Item modal category dropdown
    const modalSelect = document.getElementById('newItemCategory');
    if (modalSelect) {
        modalSelect.innerHTML = '';
        masterCategories.forEach(cat => {
            const opt = document.createElement('option');
            opt.value = cat;
            opt.innerText = cat;
            modalSelect.appendChild(opt);
        });
    }
    
    categories.forEach(cat => {
        const btn = document.createElement('button');
        btn.className = `cat-btn ${currentCategory === cat ? 'active' : ''}`;
        btn.innerText = cat === 'All' ? t('all_parts') : cat;
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

    const query = document.getElementById('searchInput')?.value.toLowerCase() || '';

    const filtered = allProducts.filter(p => {
        const matchesCategory = currentCategory === 'All' || p.category === currentCategory;
        const matchesSearch = !query || 
            (p.name && p.name.toLowerCase().includes(query)) || 
            (p.sku && p.sku.toLowerCase().includes(query)) || 
            (p.barcode && p.barcode.toLowerCase().includes(query));
        return matchesCategory && matchesSearch;
    });

    filtered.forEach(p => {
        const card = document.createElement('div');
        card.className = 'product-card';
        card.onclick = () => addToCart(p);
        
        // Priority: Item Image -> Category Icon -> Default Box
        let displayContent = '';
        const itemImage = p.image;
        const catImage = p.categoryImage;

        if (itemImage && itemImage.length > 5 && itemImage.includes('/')) {
            // It's a path to a product image
            displayContent = `<img src="${itemImage}" class="cat-icon-img" alt="${p.name}">`;
        } else if (catImage && catImage.length > 5 && catImage.includes('/')) {
            // It's a path to a category icon
            displayContent = `<img src="${catImage}" class="cat-icon-img" alt="${p.category}">`;
        } else {
            // Fallback to emoji or default box
            displayContent = `<span class="emoji-icon">${itemImage || '📦'}</span>`;
        }

        card.innerHTML = `
            <div class="card-edit-btn" onclick="event.stopPropagation(); openEditModal(${p.id})">
                <svg viewBox="0 0 24 24" width="16" height="16" stroke="currentColor" stroke-width="2.5" fill="none"><path d="M11 4H4a2 2 0 0 0-2 2v14a2 2 0 0 0 2 2h14a2 2 0 0 0 2-2v-7"></path><path d="M18.5 2.5a2.121 2.121 0 0 1 3 3L12 15l-4 1 1-4L18.5 2.5z"></path></svg>
            </div>
            <div class="product-img">${displayContent}</div>
            <div class="product-info">
                <div class="product-name">${p.name}</div>
                <div class="product-price">${formatPrice(p.price)}</div>
                <div class="product-stock ${p.stock < 5 ? 'low' : ''}">${t('stock_label')}: ${p.stock}</div>
            </div>
        `;
        grid.appendChild(card);
    });
}

function openEditModal(id) {
    const item = allProducts.find(p => p.id === id);
    if (!item) return;
    document.getElementById('modalTitle').innerText = t('modal_edit_title');
    document.getElementById('btnSubmitItem').innerText = t('modal_save_btn');
    document.getElementById('editItemId').value = item.id;
    document.getElementById('newItemName').value = item.name;
    document.getElementById('newItemCategory').value = item.category || (masterCategories.length > 0 ? masterCategories[0] : '');
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
                <div class="cart-item-price">${formatPrice(item.price)}</div>
            </div>
            <div class="qty-controls">
                <button class="qty-btn" onclick="changeQty(${item.id}, -1)">-</button>
                <div class="qty-val">${item.quantity}</div>
                <button class="qty-btn" onclick="changeQty(${item.id}, 1)">+</button>
            </div>
            <div class="cart-item-total">${formatPrice(total)}</div>
        `;
        list.appendChild(div);
    });

    const taxRate = document.getElementById('applyTax')?.checked ? 0.10 : 0;
    const tax = subtotal * taxRate;
    document.getElementById('subTotal').innerText = formatPrice(subtotal);
    document.getElementById('taxTotal').innerText = formatPrice(tax);
    document.getElementById('grandTotal').innerText = formatPrice(subtotal + tax);
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
    // Hide header on login page
    const topNav = document.querySelector('.top-nav');
    if (topNav) topNav.style.display = 'none';
}

async function handleLogin() {
    const user = document.getElementById('loginUser').value;
    const pass = document.getElementById('loginPass').value;
    const errorBox = document.getElementById('loginError');

    if (errorBox) {
        errorBox.classList.add('hidden');
        errorBox.innerText = '';
    }

    try {
        const res = await fetch(`${API_BASE}/api/login`, {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify({ username: user, password: pass })
        });

        if (res.ok) {
            const data = await res.json();
            localStorage.setItem('pos_loggedIn', 'true');
            localStorage.setItem('pos_username', data.username);
            localStorage.setItem('pos_user', data.fullName);
            document.getElementById('loginScreen').classList.add('hidden');
            // Show header after successful login
            const topNav = document.querySelector('.top-nav');
            if (topNav) topNav.style.display = 'flex';
            showToast(`Logged in as ${data.fullName}`, "success");
            globalBarcodeScanner.init(); // Activate scanner immediately on login
            initApp();
        } else {
            const msg = "Invalid username or password";
            if (errorBox) {
                errorBox.innerText = msg;
                errorBox.classList.remove('hidden');
            }
            showToast(msg, "error");
        }
    } catch (err) {
        const msg = "Login server offline";
        if (errorBox) {
            errorBox.innerText = msg;
            errorBox.classList.remove('hidden');
        }
        showToast(msg, "error");
    }
}

function handleLogout() {
    localStorage.removeItem('pos_loggedIn');
    localStorage.removeItem('pos_username');
    localStorage.removeItem('pos_user');
    document.getElementById('lockScreen').classList.add('hidden');
    document.getElementById('loginScreen').classList.remove('hidden');
    showToast("Logged out", "info");
}

function handleLock() {
    const user = localStorage.getItem('pos_user') || 'User';
    document.getElementById('lockUserDisplay').innerText = user;
    document.getElementById('lockPass').value = '';
    document.getElementById('lockScreen').classList.remove('hidden');
}

async function handleUnlock() {
    const user = localStorage.getItem('pos_username');
    const pass = document.getElementById('lockPass').value;
    
    if (!user) { handleLogout(); return; }

    try {
        const res = await fetch(`${API_BASE}/api/login`, {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify({ username: user, password: pass })
        });

        if (res.ok) {
            document.getElementById('lockScreen').classList.add('hidden');
            showToast("Session Unlocked", "success");
        } else {
            showToast("Invalid password", "error");
        }
    } catch (err) {
        showToast("Server offline", "error");
    }
}

// ── GLOBAL HID BARCODE SCANNER ──────────────────────────────────────────────
// USB/Bluetooth scanners act as keyboards: they type chars very fast then Enter.
// We detect this by checking the time between keystrokes (< 50ms = scanner).
const globalBarcodeScanner = {
    buffer: '',
    lastKeyTime: 0,
    THRESHOLD_MS: 50,   // Max ms between scanner keystrokes
    MIN_LENGTH: 3,      // Minimum barcode length to process

    init() {
        document.addEventListener('keydown', (e) => this.onKey(e));
        console.log('Global barcode scanner ready.');
    },

    onKey(e) {
        // Ignore if user is focused on an input/textarea/select
        const tag = document.activeElement?.tagName;
        if (tag === 'INPUT' || tag === 'TEXTAREA' || tag === 'SELECT') return;

        // Ignore if login screen is visible
        const loginScreen = document.getElementById('loginScreen');
        if (loginScreen && !loginScreen.classList.contains('hidden')) return;

        const lockScreen = document.getElementById('lockScreen');
        if (lockScreen && !lockScreen.classList.contains('hidden')) return;

        // Ignore if a modal is open
        const modal = document.getElementById('addItemModal');
        if (modal && !modal.classList.contains('hidden')) return;

        const now = Date.now();
        const timeSinceLast = now - this.lastKeyTime;
        this.lastKeyTime = now;

        if (e.key === 'Enter') {
            const code = this.buffer.trim();
            this.buffer = '';
            if (code.length >= this.MIN_LENGTH) {
                e.preventDefault();
                e.stopPropagation();
                this.processBarcode(code);
                // Force blur any focused button to prevent accidental re-triggers
                if (document.activeElement instanceof HTMLElement) {
                    document.activeElement.blur();
                }
            }
            return;
        }

        // If too much time has passed, this is a new sequence — reset buffer
        if (timeSinceLast > 500) this.buffer = '';

        // Only accumulate printable single characters
        if (e.key.length === 1) this.buffer += e.key;
    },

    processBarcode(code) {
        // Search by barcode first, then by SKU
        const product = allProducts.find(p => p.barcode === code) 
                     || allProducts.find(p => p.sku === code);

        if (product) {
            addToCart(product);
            showToast(`✅ Added: ${product.name}`, 'success');
        } else {
            showToast(`⚠️ Barcode not found: ${code}`, 'error');
        }
    }
};

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
            ? `<div class="notif-item"><div class="title">${t('notif_all_good')}</div></div>` 
            : '';
        lowItems.forEach(item => {
            const div = document.createElement('div');
            const isOut = item.stock <= 0;
            div.className = `notif-item ${isOut ? 'out-of-stock' : 'low-stock'}`;
            div.innerHTML = `<div class="title">${item.name}</div><div class="desc">${isOut ? t('notif_out_of_stock') : t('notif_low_stock')}: ${item.stock} left</div>`;
            list.appendChild(div);
        });
    }
}

function setupNotificationSystem() {
    const taxToggle = document.getElementById('applyTax');
    if (taxToggle) taxToggle.onchange = updateCartUI;

    // Periodic check for low stock (every 30 seconds, like desktop)
    checkLowStockAlerts();
    setInterval(checkLowStockAlerts, 30000);

    // Ensure the panel can be closed by clicking outside
    document.addEventListener('click', (e) => {
        const panel = document.getElementById('notificationPanel');
        const bell = document.getElementById('btnNotifications');
        if (panel && !panel.contains(e.target) && !bell.contains(e.target)) {
            panel.classList.add('hidden');
        }
    });
}

function showToast(msg, type = 'info') {
    const toast = document.createElement('div');
    toast.className = `toast ${type}`;
    toast.style.cssText = `position:fixed; bottom:30px; left:50%; transform:translateX(-50%); padding:12px 24px; background:rgba(20,20,20,0.95); backdrop-filter:blur(10px); border:1px solid var(--border-color); color:white; border-radius:12px; z-index:999999; font-weight:700; box-shadow:0 10px 30px rgba(0,0,0,0.5);`;
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
        fetchInventory().then(() => {
            renderProducts();
            checkLowStockAlerts();
        });
    });

    connection.on("InventoryChanged", (msg) => {
        console.log("Real-time sync: InventoryChanged", msg);
        initApp(); // Full refresh for structural changes
    });

    connection.on("LanguageChanged", (lang) => {
        console.log("Real-time sync: LanguageChanged", lang);
        if (lang && lang !== currentLang) {
            currentLang = lang;
            applyLanguage();
        }
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
        
        // Explicitly request permissions first to provide better UX
        try {
            const stream = await navigator.mediaDevices.getUserMedia({ video: true });
            stream.getTracks().forEach(track => track.stop()); // Stop immediately, just checking permission
        } catch (err) {
            console.error("Camera permission denied", err);
            showToast("Camera access denied. Please enable it in browser settings.", "error");
            return;
        }

        document.getElementById('cameraScannerModal').classList.remove('hidden');
        if (!this.scanner) this.scanner = new Html5Qrcode("reader");
        
        try {
            await this.scanner.start(
                { facingMode: this.currentFacingMode }, 
                { 
                    fps: 24, 
                    qrbox: (w, h) => { 
                        const s = Math.min(w, h) * 0.65; 
                        return { width: s, height: s }; 
                    } 
                }, 
                (t) => this.onScanSuccess(t), 
                () => {}
            );
        } catch (err) { 
            console.error("Scanner start error", err);
            showToast("Failed to start camera. It might be in use.", "error"); 
            document.getElementById('cameraScannerModal').classList.add('hidden');
        }
    },
    async stop() { 
        if (this.scanner && this.scanner.isScanning) { 
            try { await this.scanner.stop(); } catch(e) { console.warn("Scanner stop failed", e); }
        } 
        document.getElementById('cameraScannerModal').classList.add('hidden'); 
    },
    async toggleCamera() { this.currentFacingMode = (this.currentFacingMode === "environment") ? "user" : "environment"; await this.stop(); await this.start(this.target); },
    onScanSuccess(decodedText) {
        if (this.target === 'modal') {
            document.getElementById('newItemBarcode').value = decodedText;
            this.stop(); // Close after scan
        } else {
            const p = allProducts.find(x => x.barcode === decodedText);
            if (p) { 
                addToCart(p); 
                showToast(`Added: ${p.name}`, "success"); 
                this.stop(); // Close after scan
            }
        }
    }
};

// ── RETURNS SYSTEM ────────────────────────────────────────────────────────
let selectedReturnOrder = null;

async function openReturnsModal() {
    document.getElementById('returnsModal').classList.remove('hidden');
    backToOrderList();
    await fetchRecentSales();
}

async function fetchRecentSales() {
    try {
        const res = await fetch(`${API_BASE}/api/recent-sales`);
        if (res.ok) {
            const sales = await res.json();
            const container = document.getElementById('returnsOrderList');
            container.innerHTML = sales.map(s => `
                <div class="order-row" onclick="selectOrderForReturn(${s.orderId})">
                    <div>
                        <div style="font-weight:800; font-size:1rem;">Order #${s.orderId}</div>
                        <div style="font-size:0.8rem; color:var(--text-muted);">${new Date(s.date).toLocaleString()}</div>
                    </div>
                    <div style="text-align:right;">
                        <div style="font-weight:800; color:var(--accent);">${formatPrice(s.total)}</div>
                        <div style="font-size:0.75rem;">${s.customer}</div>
                    </div>
                </div>
            `).join('');
        }
    } catch (e) { showToast("Failed to fetch history", "error"); }
}

async function selectOrderForReturn(orderId) {
    try {
        const res = await fetch(`${API_BASE}/api/order-details/${orderId}`);
        if (res.ok) {
            selectedReturnOrder = { id: orderId, items: await res.json() };
            document.getElementById('returnsOrderList').classList.add('hidden');
            document.getElementById('returnsItemSelection').classList.remove('hidden');
            
            const container = document.getElementById('returnItemsList');
            container.innerHTML = `<h3>Items in Order #${orderId}</h3>` + selectedReturnOrder.items.map(i => `
                <div class="return-item-row">
                    <div style="flex:1;">
                        <div style="font-weight:700;">${i.name}</div>
                        <div style="font-size:0.8rem; color:var(--text-muted);">Purchased: ${i.qty} @ ${formatPrice(i.price)}</div>
                    </div>
                    <div>
                        <label style="font-size:0.6rem; display:block; margin-bottom:2px;">QTY TO RETURN</label>
                        <input type="number" class="return-qty-input" data-pid="${i.partId}" data-price="${i.price}" value="0" min="0" max="${i.qty}">
                    </div>
                </div>
            `).join('');
        }
    } catch (e) { showToast("Failed to load details", "error"); }
}

function backToOrderList() {
    document.getElementById('returnsOrderList').classList.remove('hidden');
    document.getElementById('returnsItemSelection').classList.add('hidden');
    selectedReturnOrder = null;
}

async function processReturn() {
    if (!selectedReturnOrder) return;
    
    const inputs = document.querySelectorAll('.return-qty-input');
    const items = [];
    inputs.forEach(input => {
        const qty = parseInt(input.value);
        if (qty > 0) {
            items.push({
                partId: parseInt(input.dataset.pid),
                qty: qty,
                refundAmount: qty * parseFloat(input.dataset.price)
            });
        }
    });

    if (items.length === 0) {
        showToast("Select at least one item to return", "warn");
        return;
    }

    const payload = {
        orderId: selectedReturnOrder.id,
        reason: document.getElementById('returnReason').value || "Customer request",
        items: items
    };

    try {
        const res = await fetch(`${API_BASE}/api/return-item`, {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify(payload)
        });

        if (res.ok) {
            showToast("Refund processed successfully", "success");
            document.getElementById('returnsModal').classList.add('hidden');
            await initApp(); // Refresh inventory
        } else {
            showToast("Failed to process return", "error");
        }
    } catch (e) { showToast("Connection error", "error"); }
}
