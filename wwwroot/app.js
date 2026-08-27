// ─── INTERNATIONALIZATION (i18n) ENGINE ───────────────────────────────────
const TRANSLATIONS = {
    en: {
        // Login
        login_title: 'Inventory Portal',
        login_subtitle: 'Sign in to manage your inventory',
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
        modal_field_name_ph: 'e.g. Salmon Fillet',
        modal_field_category: 'Category',
        modal_field_price: 'Price ($)',
        modal_field_stock: 'Current Stock (Base Unit)',
        modal_field_barcode: 'Barcode / SKU',
        modal_field_barcode_ph: 'Scan or type barcode',
        modal_field_icon: 'Icon / Image',
        modal_choose_icon_btn: 'Choose Icon / Upload Image',
        icon_picker_title: 'Choose Icon / Image',
        icon_picker_device_gallery: 'Choose from Device Gallery',
        icon_picker_or_select_icon: 'Or select a preset icon',
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
        modal_field_name_ph: 'مثال: فيليه السلمون',
        modal_field_category: 'الفئة',
        modal_field_price: 'السعر',
        modal_field_stock: 'الكمية الحالية (الوحدة الأساسية)',
        modal_field_barcode: 'الباركود / الرمز',
        modal_field_barcode_ph: 'امسح أو اكتب الباركود',
        modal_field_icon: 'أيقونة / صورة',
        modal_choose_icon_btn: 'اختر أيقونة / تحميل صورة',
        icon_picker_title: 'اختر أيقونة / صورة',
        icon_picker_device_gallery: 'اختر من معرض الجهاز',
        icon_picker_or_select_icon: 'أو اختر أيقونة جاهزة',
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
    { id: 1, name: 'Fresh Salmon Fillet', price: 18.50, stock: 40, category: 'Seafood', image: '🐟' },
    { id: 2, name: 'Garlic Butter', price: 4.25, stock: 15, category: 'Groceries', image: '🧈' },
    { id: 3, name: 'White Onion', price: 1.50, stock: 80, category: 'Vegetables', image: '🧅' },
    { id: 10, name: 'Full Kitchen Prep Service', price: 50.00, stock: 999, category: 'Services', image: '🍽️', isService: true }
];

// Mock fetch for local browser testing/verification
if (typeof window !== 'undefined' && !window.AndroidBridge) {
    console.log("Mocking fetch API for offline browser testing");
    const mockDb = {
        users: [
            { id: 1, username: 'Softio.Admin', password: 'Softio@2026!', fullName: 'Softio Super Admin', role: 'Admin', isActive: 1, dateCreated: '2026-07-02 12:00:00' },
            { id: 2, username: 'Admin', password: 'Admin.Softio', fullName: 'Test Admin', role: 'Admin', isActive: 1, dateCreated: '2026-07-02 12:00:00' },
            { id: 3, username: 'staff', password: 'Staff.Softio', fullName: 'Test Staff', role: 'Staff', isActive: 1, dateCreated: '2026-07-02 12:00:00' },
            { id: 4, username: 'production', password: 'Productio@2026!', fullName: 'Production User', role: 'Production', isActive: 1, dateCreated: '2026-07-02 12:00:00' }
        ],
        products: [
            { id: 1, name: 'Fresh Salmon Fillet', price: 18.50, stock: 40, category: 'Seafood', image: '🐟' },
            { id: 2, name: 'Garlic Butter', price: 4.25, stock: 15, category: 'Groceries', image: '🧈' },
            { id: 3, name: 'White Onion', price: 1.50, stock: 80, category: 'Vegetables', image: '🧅' },
            { id: 4, name: 'Tomato Paste', price: 2.99, stock: 50, category: 'Groceries', image: '🥫' }
        ],
        categories: ['Seafood', 'Groceries', 'Vegetables', 'Services'],
        recipes: [
            {
                id: 1,
                name: 'Garlic Butter Salmon',
                description: 'Salmon fillet cooked in fresh butter sauce',
                price: 24.99,
                totalCost: 11.25,
                parts: [
                    { partId: 4, partName: 'Fresh Salmon Fillet', qty: 1.0, unitCost: 10.00 },
                    { partId: 5, partName: 'Garlic Butter', qty: 0.5, unitCost: 2.50 }
                ]
            }
        ],
        reports: {
            revenue: 1250.50,
            orders: 45,
            outOfStock: 2,
            lowStock: 4,
            categorySales: [
                { category: 'Seafood', sales: 750.00 },
                { category: 'Groceries', sales: 300.00 },
                { category: 'Vegetables', sales: 200.50 }
            ],
            transactions: [
                { action: 'SALE', item: 'POS Sale', desc: 'Order #101 -- Total: $24.99', user: 'Admin', time: '2026-07-02 14:15' },
                { action: 'STOCK_ADD', item: 'Fresh Salmon Fillet', desc: 'Added via Import (Qty: 50)', user: 'Admin', time: '2026-07-02 13:00' }
            ],
            cogs: 550.00,
            grossProfit: 700.50,
            profitMargin: 56.02,
            recipeSummary: [
                { name: 'Garlic Butter Salmon', qtySold: 20, revenue: 499.80, cost: 225.00, profit: 274.80 }
            ],
            ingredientConsumption: [
                { name: 'Fresh Salmon Fillet', openingStock: 80.00, purchased: 50.00, usedInRecipes: 20.00, soldDirectly: 10.00, closingStock: 100.00, unit: 'kg' },
                { name: 'Garlic Butter', openingStock: 20.00, purchased: 10.00, usedInRecipes: 10.00, soldDirectly: 0.00, closingStock: 20.00, unit: 'kg' }
            ],
            stockMovement: [
                { date: '2026-07-02 13:00:00', ingredient: 'Fresh Salmon Fillet', type: 'Purchase', quantity: 50.00, unit: 'kg', remainingStock: 130.00 },
                { date: '2026-07-02 14:15:00', ingredient: 'Fresh Salmon Fillet', type: 'Recipe Consumption', quantity: 20.00, unit: 'kg', remainingStock: 110.00 },
                { date: '2026-07-02 14:15:00', ingredient: 'Garlic Butter', type: 'Recipe Consumption', quantity: 10.00, unit: 'kg', remainingStock: 20.00 }
            ],
            financialSummary: {
                revenue: 1250.50,
                purchaseCost: 150.00,
                ingredientCost: 550.00,
                grossProfit: 700.50,
                profitMargin: 56.02,
                inventoryValue: 1200.00
            },
            chartsData: {
                salesByDay: { '2026-07-01': 200, '2026-07-02': 350, '2026-07-03': 150, '2026-07-04': 400, '2026-07-05': 150.50 },
                bestRecipes: [ { name: 'Garlic Butter Salmon', qty: 20 } ],
                mostIngredients: [ { name: 'Fresh Salmon Fillet', qty: 30 } ],
                revenueTrend: [
                    { month: 'Feb 2026', revenue: 800 },
                    { month: 'Mar 2026', revenue: 950 },
                    { month: 'Apr 2026', revenue: 1100 },
                    { month: 'May 2026', revenue: 1050 },
                    { month: 'Jun 2026', revenue: 1200 },
                    { month: 'Jul 2026', revenue: 1250.50 }
                ]
            }
        }
    };

    window.fetch = async (url, options = {}) => {
        const path = url.replace(/https?:\/\/[^\/]+/, '').replace(/^\//, '');
        console.log("Mock Fetch Request:", path, options);
        
        let responseData = {};
        let status = 200;

        if (path === 'api/config') {
            responseData = { language: 'en', isArabic: false, primaryColor: '#0ea5e9', primaryRgb: '14, 165, 233' };
        } else if (path === 'api/users') {
            if (options.method === 'POST') {
                try {
                    const body = JSON.parse(options.body);
                    if (body.id) {
                        const u = mockDb.users.find(x => x.id === parseInt(body.id));
                        if (u) {
                            Object.assign(u, body);
                        } else {
                            status = 404;
                            responseData = { error: 'User not found' };
                        }
                    } else {
                        const existing = mockDb.users.find(x => x.username.toLowerCase() === body.username.toLowerCase());
                        if (existing) {
                            status = 409;
                            responseData = { error: 'Username already exists' };
                        } else {
                            body.id = mockDb.users.length + 1;
                            body.dateCreated = new Date().toISOString().replace('T', ' ').substring(0, 19);
                            mockDb.users.push(body);
                        }
                    }
                } catch (e) {
                    status = 400;
                    responseData = { error: 'Invalid request' };
                }
            } else {
                responseData = mockDb.users;
            }
        } else if (path.startsWith('api/users/') && options.method === 'DELETE') {
            const id = parseInt(path.split('/').pop());
            const u = mockDb.users.find(x => x.id === id);
            if (u) {
                mockDb.users = mockDb.users.filter(x => x.id !== id);
                responseData = { success: true };
            } else {
                status = 404;
                responseData = { error: 'User not found' };
            }
        } else if (path === 'api/login') {
            try {
                const body = JSON.parse(options.body);
                if (body.username === 'Softio.Admin' && body.password === 'Softio@2026!') {
                    responseData = { username: 'Softio.Admin', role: 'Admin', fullName: 'Softio Super Admin' };
                } else if (body.username?.toLowerCase() === 'admin' && body.password === 'Admin.Softio') {
                    responseData = { username: 'Admin', role: 'Admin', fullName: 'Test Admin' };
                } else if (body.username === 'staff' && body.password === 'Staff.Softio') {
                    responseData = { username: 'staff', role: 'Staff', fullName: 'Test Staff' };
                } else if (body.username?.toLowerCase() === 'production' && body.password === 'Productio@2026!') {
                    responseData = { username: 'production', role: 'Production', fullName: 'Production User' };
                } else {
                    status = 401;
                    responseData = { error: 'Unauthorized' };
                }
            } catch (e) {
                status = 400;
                responseData = { error: 'Invalid Request' };
            }
        } else if (path === 'api/products') {
            responseData = mockDb.products;
        } else if (path === 'api/categories') {
            responseData = mockDb.categories;
        } else if (path === 'api/currencies') {
            responseData = [{ code: 'USD', symbol: '$', rate: 1 }];
        } else if (path === 'api/recipes') {
            if (options.method === 'POST') {
                const body = JSON.parse(options.body);
                if (body.id) {
                    const r = mockDb.recipes.find(x => x.id === parseInt(body.id));
                    if (r) Object.assign(r, body);
                } else {
                    body.id = mockDb.recipes.length + 1;
                    mockDb.recipes.push(body);
                }
            }
            responseData = mockDb.recipes;
        } else if (path.startsWith('api/recipes/') && options.method === 'DELETE') {
            const id = parseInt(path.split('/').pop());
            mockDb.recipes = mockDb.recipes.filter(r => r.id !== id);
            responseData = { success: true };
        } else if (path.startsWith('api/products/') && options.method === 'DELETE') {
            const id = parseInt(path.split('/').pop());
            mockDb.products = mockDb.products.filter(p => p.id !== id);
            responseData = { success: true };
        } else if (path === 'api/add-item') {
            const body = JSON.parse(options.body);
            if (body.id) {
                const p = mockDb.products.find(x => x.id === parseInt(body.id));
                if (p) Object.assign(p, body);
            } else {
                body.id = mockDb.products.length + 1;
                mockDb.products.push(body);
            }
            responseData = { success: true };
        } else if (path === 'api/checkout') {
            responseData = { success: true, orderId: 102, total: 45.00 };
        } else if (path === 'api/import-items') {
            const body = JSON.parse(options.body);
            let imported = 0;
            let skipped = 0;

            body.items.forEach((item) => {
                if (!item.name) {
                    skipped++;
                    return;
                }

                // Check if existing
                let existing = mockDb.products.find(p => 
                    (item.barcode && p.barcode === item.barcode) ||
                    (item.sku && p.sku === item.sku) ||
                    (p.name.toLowerCase() === item.name.toLowerCase())
                );

                const pSize = 1.0;
                const conv = parseFloat(item.conversionValue) || 1.0;
                const pPrice = parseFloat(item.packPrice || item.price || 0.00);

                const stockPacks = item.stock; 
                const costPerBigUnit = pPrice;
                const costPerSmallUnit = costPerBigUnit / conv;

                if (existing) {
                    existing.name = item.name;
                    existing.sku = item.sku || existing.sku;
                    existing.barcode = item.barcode || existing.barcode;
                    existing.itemNo = item.itemNo || existing.itemNo;
                    existing.category = item.category || existing.category;
                    existing.description = item.description || existing.description;
                    existing.bigUnit = item.bigUnit || existing.bigUnit;
                    existing.smallUnit = item.smallUnit || existing.smallUnit;
                    existing.conversionValue = conv;
                    existing.packSize = pSize;
                    existing.packPrice = pPrice;
                    existing.purchasePrice = pPrice;
                    existing.price = item.price || existing.price;
                    existing.stock = stockPacks;
                    existing.minStock = item.minStock || existing.minStock;
                    existing.itemPrice = costPerBigUnit;
                    existing.piecePrice = costPerSmallUnit;
                } else {
                    mockDb.products.push({
                        id: mockDb.products.length + 1,
                        name: item.name,
                        sku: item.sku || '',
                        barcode: item.barcode || '',
                        itemNo: item.itemNo || '',
                        category: item.category || 'General',
                        description: item.description || '',
                        bigUnit: item.bigUnit || '',
                        smallUnit: item.smallUnit || '',
                        conversionValue: conv,
                        packSize: pSize,
                        packPrice: pPrice,
                        purchasePrice: pPrice,
                        price: item.price || pPrice,
                        stock: stockPacks,
                        minStock: item.minStock || 5,
                        itemPrice: costPerBigUnit,
                        piecePrice: costPerSmallUnit
                    });
                }
                imported++;
            });
            responseData = { success: true, imported, skipped };
        } else if (path === 'api/import-sales') {
            const body = JSON.parse(options.body);
            let processed = 0;
            let skipped = 0;
            const skippedNames = [];
            const deductions = [];
            body.sales.forEach(sale => {
                const cleanName = sale.recipeName.toLowerCase().replace(/[\s\t\r\n]/g, '');
                
                // 1. Try finding recipe
                const recipe = mockDb.recipes.find(r => r.name.toLowerCase().replace(/[\s\t\r\n]/g, '') === cleanName);
                if (recipe) {
                    recipe.parts.forEach(part => {
                        const prod = mockDb.products.find(p => p.id === part.partId);
                        if (prod) {
                            const pSize = parseFloat(prod.packSize) || 1.0;
                            const conv = parseFloat(prod.conversionValue) || 1.0;
                            const partUom = (part.unitOfMeasure || '').toLowerCase().trim();
                            const sUnit = (prod.smallUnit || '').toLowerCase().trim();
                            const bUnit = (prod.bigUnit || '').toLowerCase().trim();

                            const isBigPack = bUnit === 'pack' || bUnit === 'package';
                            const effectivePSize = isBigPack ? 1.0 : pSize;

                            let qtyPerRecipeConverted = part.qty;
                            if (bUnit && sUnit) {
                                if (bUnit === sUnit) {
                                    if (partUom === 'pack') {
                                        qtyPerRecipeConverted = part.qty;
                                    } else {
                                        qtyPerRecipeConverted = part.qty / effectivePSize;
                                    }
                                } else {
                                    if (partUom === sUnit) {
                                        qtyPerRecipeConverted = part.qty / (effectivePSize * conv);
                                    } else if (partUom === bUnit) {
                                        qtyPerRecipeConverted = part.qty / effectivePSize;
                                    } else if (partUom === 'pack') {
                                        qtyPerRecipeConverted = part.qty;
                                    } else {
                                        qtyPerRecipeConverted = part.qty / (effectivePSize * conv);
                                    }
                                }
                            }

                            const qtyDeducted = qtyPerRecipeConverted * sale.qtySold;
                            if (qtyDeducted <= 0) return;
                            
                            const prevStock = prod.stock;
                            prod.stock = Math.max(0, parseFloat((prod.stock - qtyDeducted).toFixed(4)));

                            const existingDeduct = deductions.find(d => d.partId === part.partId);
                            if (existingDeduct) {
                                existingDeduct.qtyDeducted = parseFloat((existingDeduct.qtyDeducted + qtyDeducted).toFixed(4));
                                existingDeduct.newStock = prod.stock;
                            } else {
                                deductions.push({
                                    partId: part.partId,
                                    partName: prod.name,
                                    qtyDeducted: qtyDeducted,
                                    previousStock: prevStock,
                                    newStock: prod.stock
                                });
                            }

                            // Log transaction
                            mockDb.reports.transactions.unshift({
                                action: 'STOCK_DEDUCT',
                                item: prod.name,
                                desc: `Deducted ${qtyDeducted.toFixed(4).replace(/\.0000$/, '')} via sales import (${recipe.name} x${sale.qtySold})`,
                                user: 'Admin',
                                time: new Date().toISOString().replace('T', ' ').slice(0, 16)
                            });
                        }
                    });
                    processed++;
                } else {
                    // 2. Try finding product directly (Ingredient Sale)
                    const prod = mockDb.products.find(p => p.name.toLowerCase().replace(/[\s\t\r\n]/g, '') === cleanName);
                    if (prod) {
                        const pSize = parseFloat(prod.packSize) || 1.0;
                        const conv = parseFloat(prod.conversionValue) || 1.0;
                        const saleUom = (sale.unitOfMeasure || '').toLowerCase().trim();
                        const sUnit = (prod.smallUnit || '').toLowerCase().trim();
                        const bUnit = (prod.bigUnit || '').toLowerCase().trim();

                        const isBigPack = bUnit === 'pack' || bUnit === 'package';
                        const effectivePSize = isBigPack ? 1.0 : pSize;

                        let qtyConverted = sale.qtySold;
                        if (bUnit && sUnit) {
                            if (bUnit === sUnit) {
                                if (saleUom === 'pack') {
                                    qtyConverted = sale.qtySold;
                                } else {
                                    qtyConverted = sale.qtySold / effectivePSize;
                                }
                            } else {
                                if (saleUom === sUnit) {
                                    qtyConverted = sale.qtySold / (effectivePSize * conv);
                                } else if (saleUom === bUnit) {
                                    qtyConverted = sale.qtySold / effectivePSize;
                                } else if (saleUom === 'pack') {
                                    qtyConverted = sale.qtySold;
                                } else {
                                    qtyConverted = sale.qtySold / (effectivePSize * conv);
                                }
                            }
                        }

                        const qtyDeducted = qtyConverted;
                        if (qtyDeducted > 0) {
                            const prevStock = prod.stock;
                            prod.stock = Math.max(0, parseFloat((prod.stock - qtyDeducted).toFixed(4)));

                            const existingDeduct = deductions.find(d => d.partId === prod.id);
                            if (existingDeduct) {
                                existingDeduct.qtyDeducted = parseFloat((existingDeduct.qtyDeducted + qtyDeducted).toFixed(4));
                                existingDeduct.newStock = prod.stock;
                            } else {
                                deductions.push({
                                    partId: prod.id,
                                    partName: prod.name,
                                    qtyDeducted: qtyDeducted,
                                    previousStock: prevStock,
                                    newStock: prod.stock
                                });
                            }

                            // Log transaction
                            mockDb.reports.transactions.unshift({
                                action: 'STOCK_DEDUCT',
                                item: prod.name,
                                desc: `Deducted ${qtyDeducted.toFixed(4).replace(/\.0000$/, '')} via sales import (${prod.name} x${sale.qtySold})`,
                                user: 'Admin',
                                time: new Date().toISOString().replace('T', ' ').slice(0, 16)
                            });
                        }
                        processed++;
                    } else {
                        skipped++;
                        skippedNames.push(sale.recipeName);
                    }
                }
            });

            responseData = { success: true, processed, skipped, skippedNames, deductions };
        } else if (path === 'api/export-csv') {
            responseData = { success: true, path: 'Downloads/mock_export.csv' };
        } else if (path.startsWith('api/reports')) {
            responseData = mockDb.reports;
        }

        return {
            ok: status >= 200 && status < 300,
            status: status,
            json: async () => responseData,
            text: async () => JSON.stringify(responseData)
        };
    };
}

let allProducts = [...DEFAULT_PRODUCTS];
let cart = [];
let currentCategory = 'All';
let currencies = [];
let currentCurrency = { code: 'USD', symbol: '$', rate: 1 };
const API_BASE = ''; 
let selectedProductIcon = '📦';
let selectedRecipeIcon = '🍲';
let iconPickerTarget = 'product'; // 'product' or 'recipe'

// CORE INITIALIZATION
document.addEventListener('DOMContentLoaded', async () => {
    // Layout Mode Detection for Mobile & Android devices
    const checkLayoutMode = () => {
        const isMobile = window.innerWidth <= 1024 || !!window.AndroidBridge;
        if (isMobile) {
            document.body.classList.add('is-mobile-layout');
        } else {
            document.body.classList.remove('is-mobile-layout');
        }
    };
    checkLayoutMode();
    window.addEventListener('resize', checkLayoutMode);

    // Call checkLoginState immediately to lock/login screen quickly without flicker
    checkLoginState();

    // 0. Fetch backend language config immediately to sync web portal language with desktop app natively before UI renders
    await fetchLanguageConfig();
    applyLanguage(); 
    


    // MODAL HANDLERS
    safeListen('btnOpenAddModal', 'click', () => {
        document.getElementById('modalTitle').innerText = t('modal_add_title');
        document.getElementById('btnSubmitItem').innerText = t('modal_add_btn');
        document.getElementById('editItemId').value = '';
        document.getElementById('newItemNo').value = '';
        document.getElementById('newItemName').value = '';
        document.getElementById('newItemCategory').value = '';
        document.getElementById('newItemStock').value = '0';
        document.getElementById('newItemMinStock').value = '5';
        document.getElementById('newItemBigUnit').value = '';
        document.getElementById('newItemSmallUnit').value = '';
        document.getElementById('newItemConversionValue').value = '1';
        document.getElementById('newItemPackSize').value = '';
        document.getElementById('newItemPackPrice').value = '';
        document.getElementById('newItemBarcode').value = '';
        
        selectedProductIcon = '📦';
        updateIconPreview('product', '📦');
        
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

    // Handle file input selection
    safeListen('iconFileInput', 'change', function(e) {
        const file = e.target.files[0];
        if (!file) return;

        const reader = new FileReader();
        reader.onload = function(evt) {
            const img = new Image();
            img.onload = function() {
                const canvas = document.createElement('canvas');
                const maxDimension = 200;
                let width = img.width;
                let height = img.height;
                
                if (width > height) {
                    if (width > maxDimension) {
                        height = Math.round((height * maxDimension) / width);
                        width = maxDimension;
                    }
                } else {
                    if (height > maxDimension) {
                        width = Math.round((width * maxDimension) / height);
                        height = maxDimension;
                    }
                }
                
                canvas.width = width;
                canvas.height = height;
                const ctx = canvas.getContext('2d');
                ctx.drawImage(img, 0, 0, width, height);
                
                const dataUrl = canvas.toDataURL('image/jpeg', 0.8);
                if (iconPickerTarget === 'product') {
                    selectedProductIcon = dataUrl;
                    updateIconPreview('product', dataUrl);
                } else {
                    selectedRecipeIcon = dataUrl;
                    updateIconPreview('recipe', dataUrl);
                }
                closeIconPickerModal();
                // Reset file input so same file can be chosen again if needed
                document.getElementById('iconFileInput').value = '';
            };
            img.src = evt.target.result;
        };
        reader.readAsDataURL(file);
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
    initApp().then(() => {
        switchTab('inventory');
    });
});

// UTILITIES
function safeListen(id, event, callback) {
    const el = document.getElementById(id);
    if (el) el.addEventListener(event, callback);
}

async function fetchLanguageConfig() {
    try {
        const res = await fetch(`${API_BASE}/api/config?_=${Date.now()}`);
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

let allRecipes = [];

function refreshActiveTab() {
    const activeTab = document.querySelector('.nav-tab.active');
    if (activeTab) {
        if (activeTab.id === 'tabBtnInventory') {
            loadInventoryTable();
        } else if (activeTab.id === 'tabBtnRecipes') {
            loadRecipesTab();
        } else if (activeTab.id === 'tabBtnReports') {
            loadReportsData();
        }
    }
}

async function initApp() {
    try {
        await fetchLanguageConfig();
        await fetchInventory();
        await fetchCategories();
        await fetchCurrencies();
        await fetchRecipes();
        checkLowStockAlerts();
    } catch (e) {
        console.error("Init failed", e);
    }
    refreshActiveTab();
}

async function fetchCurrencies() {
    try {
        const res = await fetch(`${API_BASE}/api/currencies?_=${Date.now()}`);
        if (res.ok) {
            currencies = await res.json();
            const select = document.getElementById('currencySelect');
            if (select) {
                select.innerHTML = currencies.map(c => `<option value="${c.code}" ${c.code === currentCurrency.code ? 'selected' : ''}>${c.code} (${c.symbol})</option>`).join('');
            }
        }
    } catch (e) { console.error("Currencies failed", e); }
}

function formatDynamicNumber(val, maxDecimals = 5) {
    let str = val.toFixed(maxDecimals);
    str = str.replace(/0+$/, '');
    if (str.endsWith('.')) {
        return str + '00';
    }
    const parts = str.split('.');
    if (parts.length > 1 && parts[1].length < 2) {
        return val.toFixed(2);
    }
    return str;
}

function formatPrice(usdPrice, decimals = 2) {
    const converted = usdPrice * currentCurrency.rate;
    if (decimals === 'auto') {
        return `${currentCurrency.symbol}${formatDynamicNumber(converted)}`;
    }
    return `${currentCurrency.symbol}${converted.toFixed(decimals)}`;
}

async function fetchInventory() {
    try {
        const res = await fetch(`${API_BASE}/api/products?_=${Date.now()}`);
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
        const res = await fetch(`${API_BASE}/api/categories?_=${Date.now()}`);
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

    const categories = ['All', ...masterCategories, 'Recipes'];
    container.innerHTML = '';
    
    // Sync the Category datalist
    const datalist = document.getElementById('categoryDatalist');
    if (datalist) {
        datalist.innerHTML = '';
        masterCategories.forEach(cat => {
            const opt = document.createElement('option');
            opt.value = cat;
            datalist.appendChild(opt);
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

    if (currentCategory === 'Recipes') {
        const filtered = allRecipes.filter(r => {
            return !query || (r.name && r.name.toLowerCase().includes(query));
        });

        filtered.forEach(r => {
            const card = document.createElement('div');
            card.className = 'product-card recipe-product-card';
            card.onclick = () => addRecipeToCart(r);
            const subtext = r.categoryName ? `Recipe • ${r.categoryName}` : "Recipe";
            const recipeImage = r.image;
            let displayContent = `<span class="emoji-icon">🍲</span>`;
            if (recipeImage) {
                if (isImagePath(recipeImage)) {
                    displayContent = `<img src="${recipeImage}" class="cat-icon-img" alt="${r.name}">`;
                } else {
                    displayContent = `<span class="emoji-icon">${recipeImage}</span>`;
                }
            }
            card.innerHTML = `
                <div class="product-img">${displayContent}</div>
                <div class="product-info">
                    <div class="product-name">${r.name}</div>
                    <div class="product-price">${formatPrice(r.price)}</div>
                    <div class="product-stock" style="color:var(--accent);">${subtext}</div>
                </div>
            `;
            grid.appendChild(card);
        });
        return;
    }

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
        
        // Priority: Item Image (Path/Data URL) -> Item Emoji -> Category Icon -> Default Box
        let displayContent = '';
        const itemImage = p.image;
        const catImage = p.categoryImage;

        if (itemImage) {
            if (isImagePath(itemImage)) {
                displayContent = `<img src="${itemImage}" class="cat-icon-img" alt="${p.name}">`;
            } else {
                displayContent = `<span class="emoji-icon">${itemImage}</span>`;
            }
        } else if (catImage && isImagePath(catImage)) {
            displayContent = `<img src="${catImage}" class="cat-icon-img" alt="${p.category}">`;
        } else {
            displayContent = `<span class="emoji-icon">📦</span>`;
        }

        card.innerHTML = `
            <div class="card-edit-btn" onclick="event.stopPropagation(); openEditModal(${p.id})">
                <svg viewBox="0 0 24 24" width="16" height="16" stroke="currentColor" stroke-width="2.5" fill="none"><path d="M11 4H4a2 2 0 0 0-2 2v14a2 2 0 0 0 2 2h14a2 2 0 0 0 2-2v-7"></path><path d="M18.5 2.5a2.121 2.121 0 0 1 3 3L12 15l-4 1 1-4L18.5 2.5z"></path></svg>
            </div>
            <div class="product-img">${displayContent}</div>
            <div class="product-info">
                <div class="product-name">${p.name}</div>
                <div class="product-price">${formatPrice(p.price)}</div>
                <div class="product-stock ${p.stock < (p.minStock || 5) ? 'low' : ''}">${t('stock_label')}: ${p.stock} ${p.unitOfMeasure || 'pcs'}</div>
            </div>
        `;
        grid.appendChild(card);
    });

    // Also display recipes that belong to this category!
    const filteredRecipes = allRecipes.filter(r => {
        const matchesCategory = currentCategory === 'All' || r.categoryName === currentCategory;
        const matchesSearch = !query || (r.name && r.name.toLowerCase().includes(query));
        return matchesCategory && matchesSearch;
    });

    filteredRecipes.forEach(r => {
        const card = document.createElement('div');
        card.className = 'product-card recipe-product-card';
        card.onclick = () => addRecipeToCart(r);
        const subtext = r.categoryName ? `Recipe • ${r.categoryName}` : "Recipe";
        const recipeImage = r.image;
        let displayContent = `<span class="emoji-icon">🍲</span>`;
        if (recipeImage) {
            if (isImagePath(recipeImage)) {
                displayContent = `<img src="${recipeImage}" class="cat-icon-img" alt="${r.name}">`;
            } else {
                displayContent = `<span class="emoji-icon">${recipeImage}</span>`;
            }
        }
        card.innerHTML = `
            <div class="product-img">${displayContent}</div>
            <div class="product-info">
                <div class="product-name">${r.name}</div>
                <div class="product-price">${formatPrice(r.price)}</div>
                <div class="product-stock" style="color:var(--accent);">${subtext}</div>
            </div>
        `;
        grid.appendChild(card);
    });
}

window.openAddModalDirect = function() {
    document.getElementById('modalTitle').innerText = t('modal_add_title');
    document.getElementById('btnSubmitItem').innerText = t('modal_add_btn');
    document.getElementById('editItemId').value = '';
    document.getElementById('newItemNo').value = '';
    document.getElementById('newItemName').value = '';
    document.getElementById('newItemCategory').value = '';
    document.getElementById('newItemStock').value = '0';
    document.getElementById('newItemMinStock').value = '5';
    document.getElementById('newItemBigUnit').value = '';
    document.getElementById('newItemSmallUnit').value = '';
    document.getElementById('newItemConversionValue').value = '1';
    document.getElementById('newItemPackPrice').value = '';
    document.getElementById('newItemBarcode').value = '';
    
    const costGroup = document.getElementById('ingredientCostInputGroup');
    if (costGroup) costGroup.style.display = shouldShowCost() ? 'block' : 'none';
    
    selectedProductIcon = '📦';
    updateIconPreview('product', '📦');
    
    document.getElementById('addItemModal').classList.remove('hidden');
};

function openEditModal(id) {
    const item = allProducts.find(p => p.id === id);
    if (!item) return;
    document.getElementById('modalTitle').innerText = t('modal_edit_title');
    document.getElementById('btnSubmitItem').innerText = t('modal_save_btn');
    document.getElementById('editItemId').value = item.id;
    document.getElementById('newItemNo').value = item.itemNo || '';
    document.getElementById('newItemName').value = item.name;
    document.getElementById('newItemCategory').value = item.category || (masterCategories.length > 0 ? masterCategories[0] : '');
    document.getElementById('newItemStock').value = item.stock;
    document.getElementById('newItemMinStock').value = item.minStock !== undefined ? item.minStock : 5;
    document.getElementById('newItemBarcode').value = item.barcode || '';

    document.getElementById('newItemBigUnit').value = item.bigUnit || '';
    document.getElementById('newItemSmallUnit').value = item.smallUnit || '';
    document.getElementById('newItemConversionValue').value = item.conversionValue || '1';
    document.getElementById('newItemPackPrice').value = item.packPrice || item.price || '';

    selectedProductIcon = item.image || '📦';
    updateIconPreview('product', selectedProductIcon);

    const costGroup = document.getElementById('ingredientCostInputGroup');
    if (costGroup) costGroup.style.display = shouldShowCost() ? 'block' : 'none';

    document.getElementById('addItemModal').classList.remove('hidden');
}

async function submitNewItem() {
    const editId = document.getElementById('editItemId').value;
    const parsedId = editId ? parseInt(editId) : null;

    const baseUnitPrice = parseFloat(document.getElementById('newItemPackPrice').value) || 0;
    const minStock = parseFloat(document.getElementById('newItemMinStock').value) || 5;

    const bigUnit = document.getElementById('newItemBigUnit').value.trim();
    const smallUnit = document.getElementById('newItemSmallUnit').value.trim();
    const conversionValue = parseFloat(document.getElementById('newItemConversionValue').value) || 1.0;
    
    const itemPrice = baseUnitPrice;
    const piecePrice = conversionValue > 0 ? (baseUnitPrice / conversionValue) : baseUnitPrice;

    const itemData = {
        itemNo: document.getElementById('newItemNo').value,
        name: document.getElementById('newItemName').value,
        category: document.getElementById('newItemCategory').value,
        price: baseUnitPrice,
        stock: parseFloat(document.getElementById('newItemStock').value) || 0,
        barcode: document.getElementById('newItemBarcode').value,
        unitOfMeasure: bigUnit,
        stockType: 'Piece',
        packItemsNumber: 1,
        packPrice: baseUnitPrice,
        itemPrice: itemPrice,
        piecePrice: piecePrice,
        minStock: minStock,
        bigUnit: bigUnit,
        smallUnit: smallUnit,
        conversionValue: conversionValue,
        packSize: 1.0,
        image: selectedProductIcon
    };

    if (!itemData.name || isNaN(itemData.price)) {
        showToast("Please fill Name and Price", "error");
        return;
    }

    try {
        const res = await fetch(`${API_BASE}/api/add-item`, {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify({ ...itemData, id: parsedId })
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
    const existing = cart.find(item => item.id === product.id && item.itemType === 'Part');
    if (existing) {
        existing.quantity++;
    } else {
        let defaultUom = product.unitOfMeasure || 'pcs';
        let initialPrice = product.price;
        const convVal = parseFloat(product.conversionValue) || 1.0;
        if (product.bigUnit && product.smallUnit) {
            defaultUom = product.smallUnit.toLowerCase();
            initialPrice = product.price / convVal;
        }
        cart.push({ ...product, quantity: 1, itemType: 'Part', selectedUom: defaultUom, price: initialPrice, basePrice: product.price });
    }
    updateCartUI();
}

function addRecipeToCart(recipe) {
    const existing = cart.find(item => item.id === recipe.id && item.itemType === 'Recipe');
    if (existing) {
        existing.quantity++;
    } else {
        cart.push({
            id: recipe.id,
            name: recipe.name,
            price: recipe.price,
            quantity: 1,
            itemType: 'Recipe',
            recipeId: recipe.id,
            image: '🍲'
        });
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

        let uomSelectHtml = '';
        if (item.bigUnit && item.smallUnit) {
            uomSelectHtml = `
                <select class="cart-item-uom" onchange="changeCartItemUom(${item.id}, this.value, '${item.itemType || 'Part'}')" style="padding:2px 4px; background:rgba(0,0,0,0.3); color:white; border:1px solid var(--border); border-radius:4px; font-size:0.75rem; margin-top:4px; outline:none; cursor:pointer; font-weight:bold;">
                    <option value="${item.bigUnit.toLowerCase()}" ${item.selectedUom === item.bigUnit.toLowerCase() ? 'selected' : ''}>${item.bigUnit}</option>
                    <option value="${item.smallUnit.toLowerCase()}" ${item.selectedUom === item.smallUnit.toLowerCase() ? 'selected' : ''}>${item.smallUnit}</option>
                </select>
            `;
        }

        div.innerHTML = `
            <div class="cart-item-info">
                <div class="cart-item-name">${item.name}</div>
                ${uomSelectHtml}
                <div class="cart-item-price">${formatPrice(item.price)}</div>
            </div>
            <div class="qty-controls">
                <button class="qty-btn" onclick="changeQty(${item.id}, -1, '${item.itemType || 'Part'}')">-</button>
                <div class="qty-val">${item.quantity}</div>
                <button class="qty-btn" onclick="changeQty(${item.id}, 1, '${item.itemType || 'Part'}')">+</button>
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

    // Update floating cart badge count
    const totalQty = cart.reduce((sum, item) => sum + item.quantity, 0);
    const floatCount = document.getElementById('cartFloatingCount');
    if (floatCount) {
        floatCount.innerText = totalQty;
        const btn = document.getElementById('floatingCartBtn');
        if (btn) {
            if (totalQty > 0) {
                btn.classList.add('visible');
            } else {
                btn.classList.remove('visible');
                const sidebar = document.querySelector('.cart-sidebar');
                if (sidebar && sidebar.classList.contains('open')) {
                    toggleCartDrawer();
                }
            }
        }
    }
}

window.changeCartItemUom = function(id, val, itemType = 'Part') {
    const item = cart.find(x => x.id === id && x.itemType === itemType);
    if (!item) return;
    
    item.selectedUom = val;
    const basePrice = item.basePrice || item.price;
    const convVal = parseFloat(item.conversionValue) || 1.0;
    
    if (val === item.bigUnit.toLowerCase()) {
        item.price = basePrice;
    } else if (val === item.smallUnit.toLowerCase()) {
        item.price = basePrice / convVal;
    }
    updateCartUI();
};

function changeQty(id, delta, itemType = 'Part') {
    const item = cart.find(x => x.id === id && x.itemType === itemType);
    if (!item) return;

    if (item.quantity + delta <= 0) {
        cart = cart.filter(x => !(x.id === id && x.itemType === itemType));
    } else {
        if (itemType === 'Part') {
            const product = allProducts.find(p => p.id === id);
            if (delta > 0 && product && item.quantity >= product.stock && !product.isService) {
                showToast("No more stock available", "warn");
                return;
            }
        }
        item.quantity += delta;
    }
    updateCartUI();
}

function clearCart() { cart = []; updateCartUI(); }

async function processCheckout() {
    if (cart.length === 0) return;
    
    try {
        const payloadItems = cart.map(i => ({
            id: i.itemType === 'Recipe' ? 0 : i.id,
            name: i.name,
            qty: i.quantity,
            price: i.price,
            itemType: i.itemType || 'Part',
            recipeId: i.itemType === 'Recipe' ? i.id : null,
            unitOfMeasure: i.itemType === 'Recipe' ? '' : (i.selectedUom || '')
        }));

        const res = await fetch(`${API_BASE}/api/checkout`, {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify({ items: payloadItems })
        });

        if (res.ok) {
            cart = [];
            updateCartUI();
            await fetchInventory();
            await fetchRecipes();
            renderProducts();
            showToast("Transaction Complete!", "success");
        } else {
            const err = await res.json();
            showToast(err.error || "Checkout failed", "error");
        }
    } catch (err) {
        showToast("Connection error", "error");
    }
}

// LOGIN SYSTEM
function checkLoginState() {
    const loggedIn = sessionStorage.getItem('pos_loggedIn') === 'true';
    if (loggedIn) {
        document.getElementById('loginScreen').classList.add('hidden');
        // Show header after restoring session
        const topNav = document.querySelector('.top-nav');
        if (topNav) topNav.style.display = 'flex';
        updateUIForRole();
        globalBarcodeScanner.init();

        const username = sessionStorage.getItem('pos_username');
        const role = sessionStorage.getItem('pos_role');

        fetch(`${API_BASE}/api/sync-session`, {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify({ username, role })
        }).catch(() => {}).then(() => {
            initApp().then(() => {
                switchTab('inventory');
            });
        });
    } else {
        sessionStorage.removeItem('pos_loggedIn');
        sessionStorage.removeItem('pos_username');
        sessionStorage.removeItem('pos_user');
        sessionStorage.removeItem('pos_role');
        document.getElementById('loginScreen').classList.remove('hidden');
        // Hide header on login page
        const topNav = document.querySelector('.top-nav');
        if (topNav) topNav.style.display = 'none';
        updateUIForRole();
    }
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
            sessionStorage.setItem('pos_loggedIn', 'true');
            sessionStorage.setItem('pos_username', data.username);
            sessionStorage.setItem('pos_user', data.fullName);
            sessionStorage.setItem('pos_role', data.role);
            updateUIForRole();
            document.getElementById('loginScreen').classList.add('hidden');
            // Show header after successful login
            const topNav = document.querySelector('.top-nav');
            if (topNav) topNav.style.display = 'flex';
            showToast(`Logged in as ${data.fullName}`, "success");
            globalBarcodeScanner.init(); // Activate scanner immediately on login
            initApp().then(() => {
                switchTab('inventory');
            });
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
    fetch(`${API_BASE}/api/logout`, { method: 'POST' }).catch(() => {});
    sessionStorage.removeItem('pos_loggedIn');
    sessionStorage.removeItem('pos_username');
    sessionStorage.removeItem('pos_user');
    sessionStorage.removeItem('pos_role');
    updateUIForRole();
    document.getElementById('lockScreen').classList.add('hidden');
    document.getElementById('loginScreen').classList.remove('hidden');
    showToast("Logged out", "info");
}

function handleLock() {
    handleLogout();
}

async function handleUnlock() {
    const user = sessionStorage.getItem('pos_username');
    const pass = document.getElementById('lockPass').value;
    
    if (!user) { handleLogout(); return; }

    try {
        const res = await fetch(`${API_BASE}/api/login`, {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify({ username: user, password: pass })
        });

        if (res.ok) {
            const data = await res.json();
            sessionStorage.setItem('pos_role', data.role);
            updateUIForRole();
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
            refreshActiveTab();
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
        const res = await fetch(`${API_BASE}/api/recent-sales?_=${Date.now()}`);
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
        const res = await fetch(`${API_BASE}/api/order-details/${orderId}?_=${Date.now()}`);
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

// ─── TAB ROUTING SYSTEM ───────────────────────────────────────────────
function switchTab(tabId) {
    const username = (sessionStorage.getItem('pos_username') || '').toLowerCase().trim();
    const role = (sessionStorage.getItem('pos_role') || '').toLowerCase().trim();
    const isSuperAdmin = (username === 'softio.admin');
    const isProductionUser = (username === 'production' || role === 'production');

    if (isProductionUser && (tabId === 'sales' || tabId === 'stock' || tabId === 'reports')) {
        showToast("Access Denied: Production role restricted.", "error");
        return;
    }
    if (!isSuperAdmin && !isProductionUser && tabId === 'production') {
        showToast("Access Denied: Production Page is restricted.", "error");
        return;
    }

    const searchBar = document.getElementById('topSearchBar');
    const btnOpenAdd = document.getElementById('btnOpenAddModal');

    if (searchBar) {
        searchBar.style.display = 'none';
    }
    if (btnOpenAdd) {
        btnOpenAdd.style.display = 'none';
    }

    // Save previous tab ID if switching away from regular tabs to profile
    const activeTabBtn = document.querySelector('.nav-tab.active');
    if (activeTabBtn && activeTabBtn.id !== 'tabBtnUserMenu') {
        window.lastActiveTabId = activeTabBtn.id.replace('tabBtn', '').toLowerCase();
    }

    document.querySelectorAll('.tab-content-panel').forEach(panel => {
        panel.classList.add('hidden');
        panel.classList.remove('active-panel');
    });

    document.querySelectorAll('.nav-tab').forEach(btn => {
        btn.classList.remove('active');
    });

    let panelId = 'tabContentInventory';
    let btnId = 'tabBtnInventory';

    if (tabId === 'inventory') { panelId = 'tabContentInventory'; btnId = 'tabBtnInventory'; loadInventoryTable(); }
    else if (tabId === 'recipes') { panelId = 'tabContentRecipes'; btnId = 'tabBtnRecipes'; loadRecipesTab(); }
    else if (tabId === 'production') { panelId = 'tabContentProduction'; btnId = 'tabBtnProduction'; loadProductionTab(); }
    else if (tabId === 'stock') { panelId = 'tabContentStock'; btnId = 'tabBtnStock'; loadStockTab(); }
    else if (tabId === 'sales') { panelId = 'tabContentSales'; btnId = 'tabBtnSales'; loadSalesTab(); }
    else if (tabId === 'reports') { panelId = 'tabContentReports'; btnId = 'tabBtnReports'; loadReportsData(); }
    else if (tabId === 'users') { panelId = 'tabContentUsers'; btnId = 'tabBtnUsers'; loadUsersTab(); }
    else if (tabId === 'userProfile') { panelId = 'tabContentUserProfile'; btnId = 'tabBtnUserMenu'; loadWebUserProfile(); }
    else if (tabId === 'info') { panelId = 'tabContentInfo'; btnId = 'tabBtnInfo'; loadInfoTab(); }

    const activePanel = document.getElementById(panelId);
    if (activePanel) {
        activePanel.classList.remove('hidden');
        activePanel.classList.add('active-panel');
    }

    const activeBtn = document.getElementById(btnId);
    if (activeBtn) {
        activeBtn.classList.add('active');
    }
}

// ─── INVENTORY MANAGEMENT ─────────────────────────────────────────────
function populateInventoryCategoryFilter() {
    const filterSelect = document.getElementById('inventoryCategoryFilter');
    if (!filterSelect) return;

    const currentVal = filterSelect.value || 'All';

    // Collect unique categories
    const categories = new Set();
    allProducts.forEach(p => {
        if (p.category) categories.add(p.category);
    });

    if (typeof masterCategories !== 'undefined' && Array.isArray(masterCategories)) {
        masterCategories.forEach(c => {
            if (c) categories.add(c);
        });
    }

    // Sort categories
    const sortedCats = Array.from(categories).sort();

    // Rebuild options
    let html = '<option value="All">All Categories</option>';
    sortedCats.forEach(cat => {
        html += `<option value="${cat}">${cat}</option>`;
    });
    filterSelect.innerHTML = html;

    // Restore value
    if (currentVal === 'All' || sortedCats.includes(currentVal)) {
        filterSelect.value = currentVal;
    } else {
        filterSelect.value = 'All';
    }
}

window.shouldShowCost = function() {
    const role = sessionStorage.getItem('pos_role') || 'Guest';
    return role.toLowerCase() !== 'staff' && role.toLowerCase() !== 'employee';
};

window.filterInventoryByCategory = function() {
    loadInventoryTable();
};

function loadInventoryTable() {
    const grid = document.getElementById('inventoryGrid');
    if (!grid) return;

    populateInventoryCategoryFilter();

    const filterSelect = document.getElementById('inventoryCategoryFilter');
    const selectedCategory = filterSelect ? filterSelect.value : 'All';

    grid.innerHTML = '';

    const filteredProducts = selectedCategory === 'All' 
        ? allProducts 
        : allProducts.filter(p => p.category === selectedCategory);

    filteredProducts.forEach(p => {
        const card = document.createElement('div');
        card.className = 'product-card';
        
        // Priority: Item Image (Path/Data URL) -> Item Emoji -> Category Icon -> Default Box
        let displayContent = '';
        const itemImage = p.image;
        const catImage = p.categoryImage;

        let stockDetailsHtml = '';
        if (shouldShowCost()) {
            if (p.bigUnit && p.smallUnit) {
                stockDetailsHtml = `
                    <div style="font-size: 0.72rem; color: var(--text-muted); margin-top: 4px; line-height: 1.25;">
                        <span style="font-weight: 600; color: var(--text-main);">Cost</span><br/>
                        Base: <span style="color: var(--accent); font-weight: 600;">${formatPrice(p.price)}/${p.bigUnit}</span><br/>
                        Sub: <span style="color: var(--accent); font-weight: 600;">${formatPrice(p.piecePrice || 0, 'auto')}/${p.smallUnit}</span>
                    </div>
                `;
            } else {
                stockDetailsHtml = `
                    <div style="font-size: 0.72rem; color: var(--text-muted); margin-top: 4px; line-height: 1.25;">
                        <span style="font-weight: 600; color: var(--text-main);">Cost</span><br/>
                        Base: <span style="color: var(--accent); font-weight: 600;">${formatPrice(p.price)}</span>
                    </div>
                `;
            }
        }

        if (itemImage) {
            if (isImagePath(itemImage)) {
                displayContent = `<img src="${itemImage}" class="cat-icon-img" alt="${p.name}">`;
            } else {
                displayContent = `<span class="emoji-icon">${itemImage}</span>`;
            }
        } else if (catImage && isImagePath(catImage)) {
            displayContent = `<img src="${catImage}" class="cat-icon-img" alt="${p.category}">`;
        } else {
            displayContent = `<span class="emoji-icon">📦</span>`;
        }

        const currentStockVal = parseFloat(p.stock || 0).toFixed(2);
        const currentStockUom = p.bigUnit || 'pcs';
        const minStockVal = parseFloat(p.minStock || 0).toFixed(2);

        card.innerHTML = `
            <div class="card-edit-btn" onclick="event.stopPropagation(); openEditModal(${p.id})" title="Edit" style="position: absolute; top: 6px; right: 6px; width: 24px; height: 24px; background: rgba(255,255,255,0.05); border-radius: 6px; display: flex; align-items: center; justify-content: center; color: var(--text-muted); transition: all 0.2s; z-index: 5; cursor: pointer;">
                <svg viewBox="0 0 24 24" width="13" height="13" stroke="currentColor" stroke-width="2.5" fill="none"><path d="M11 4H4a2 2 0 0 0-2 2v14a2 2 0 0 0 2 2h14a2 2 0 0 0 2-2v-7"></path><path d="M18.5 2.5a2.121 2.121 0 0 1 3 3L12 15l-4 1 1-4L18.5 2.5z"></path></svg>
            </div>
            <div class="card-delete-btn" onclick="event.stopPropagation(); deleteIngredient(${p.id})" title="Delete" style="position: absolute; top: 6px; left: 6px; width: 24px; height: 24px; background: rgba(239, 68, 68, 0.05); border-radius: 6px; display: flex; align-items: center; justify-content: center; color: var(--danger); transition: all 0.2s; z-index: 5; cursor: pointer;">
                <svg viewBox="0 0 24 24" width="13" height="13" stroke="currentColor" stroke-width="2.5" fill="none"><polyline points="3 6 5 6 21 6"></polyline><path d="M19 6v14a2 2 0 0 1-2 2H7a2 2 0 0 1-2-2V6m3 0V4a2 2 0 0 1 2-2h4a2 2 0 0 1 2 2v2"></path><line x1="10" y1="11" x2="10" y2="17"></line><line x1="14" y1="11" x2="14" y2="17"></line></svg>
            </div>
            <div class="product-img" style="margin-top: 20px;">${displayContent}</div>
            <div class="product-info" style="text-align: center;">
                <div class="product-name" style="font-weight: 700; margin-bottom: 4px;">${p.name}</div>
                <div style="font-size: 0.72rem; color: var(--text-muted); margin-bottom: 4px;">Item No: <span style="color: var(--text-main); font-weight: 600;">${p.itemNo || 'N/A'}</span></div>
                ${stockDetailsHtml}
                <div class="product-stock ${p.stock < (p.minStock || 5) ? 'low' : ''}" style="font-size: 0.75rem; font-weight: 700; margin-top: 6px; line-height: 1.25;">
                    <span style="font-weight: 600; color: var(--text-muted); font-size: 0.7rem; text-transform: uppercase;">Stock</span><br/>
                    <span style="font-size: 0.85rem; color: var(--text-main); font-weight: 700;">${currentStockVal} ${currentStockUom}</span><br/>
                    <span style="font-weight: 400; color: var(--text-muted); font-size: 0.7rem;">Min: ${minStockVal} ${currentStockUom}</span>
                </div>
            </div>
        `;
        grid.appendChild(card);
    });
}

function openAddModalDirect() {
    window.openAddModalDirect();
}

function showDeleteConfirm(title, message, onConfirm, confirmText = 'Delete') {
    const overlay = document.createElement('div');
    overlay.className = 'overlay';
    overlay.style.zIndex = '99999';
    overlay.style.display = 'flex';
    overlay.style.alignItems = 'center';
    overlay.style.justifyContent = 'center';
    overlay.style.background = 'rgba(0, 0, 0, 0.4)';
    overlay.style.backdropFilter = 'blur(4px)';

    const card = document.createElement('div');
    card.className = 'scanner-card';
    card.style.maxWidth = '360px';
    card.style.padding = '24px';
    card.style.borderRadius = '16px';
    card.style.background = 'var(--bg-secondary)';
    card.style.border = '1px solid var(--border-color)';
    card.style.boxShadow = '0 10px 30px rgba(0,0,0,0.15)';
    card.style.textAlign = 'center';

    card.innerHTML = `
        <h3 style="margin-top:0; color:var(--text-main); font-size:1.2rem; font-weight:800;">${title}</h3>
        <p style="color:var(--text-muted); font-size:0.9rem; margin:16px 0 24px 0; line-height:1.5;">${message}</p>
        <div style="display:flex; gap:12px; justify-content:center;">
            <button class="btn-clear" id="confirmCancelBtn" style="flex:1; height:40px; border:1px solid var(--border-color); color:var(--text-main); border-radius:8px; font-weight:600; cursor:pointer;">Cancel</button>
            <button class="btn-primary" id="confirmOkBtn" style="flex:1; height:40px; background:var(--danger) !important; color:white; border:none; border-radius:8px; font-weight:600; cursor:pointer;">${confirmText}</button>
        </div>
    `;

    overlay.appendChild(card);
    document.body.appendChild(overlay);

    const closeConfirm = () => {
        document.body.removeChild(overlay);
    };

    overlay.querySelector('#confirmCancelBtn').onclick = closeConfirm;
    overlay.querySelector('#confirmOkBtn').onclick = () => {
        closeConfirm();
        onConfirm();
    };
}

function deleteIngredient(id) {
    showDeleteConfirm(
        "Delete Ingredient",
        "Are you sure you want to delete this ingredient?",
        async () => {
            try {
                const res = await fetch(`${API_BASE}/api/products/${id}`, { method: 'DELETE' });
                if (res.ok) {
                    showToast("Ingredient deleted successfully", "success");
                    await initApp();
                    loadInventoryTable();
                } else {
                    showToast("Failed to delete ingredient", "error");
                }
            } catch (e) { showToast("Connection error", "error"); }
        }
    );
}

// ─── RECIPES MANAGEMENT ───────────────────────────────────────────────
async function fetchRecipes() {
    try {
        const res = await fetch(`${API_BASE}/api/recipes?_=${Date.now()}`);
        if (res.ok) {
            const data = await res.json();
            if (data && data.error) {
                showToast(`Error loading recipes: ${data.error}`, "error");
                allRecipes = [];
            } else {
                allRecipes = Array.isArray(data) ? data : [];
            }
        } else {
            console.error("Fetch recipes failed with status", res.status);
            allRecipes = [];
        }
    } catch (e) { 
        console.error("Fetch recipes failed", e); 
        allRecipes = [];
    }
}

function loadRecipesTab() {
    const grid = document.getElementById('recipesGrid');
    if (!grid) return;
    grid.innerHTML = '';

    allRecipes.forEach(r => {
        const card = document.createElement('div');
        card.className = 'product-card';

        card.innerHTML = `
            <div class="card-edit-btn" onclick="event.stopPropagation(); editRecipe(${r.id})" title="Edit" style="position: absolute; top: 6px; right: 6px; width: 24px; height: 24px; background: rgba(255,255,255,0.05); border-radius: 6px; display: flex; align-items: center; justify-content: center; color: var(--text-muted); transition: all 0.2s; z-index: 5; cursor: pointer;">
                <svg viewBox="0 0 24 24" width="13" height="13" stroke="currentColor" stroke-width="2.5" fill="none"><path d="M11 4H4a2 2 0 0 0-2 2v14a2 2 0 0 0 2 2h14a2 2 0 0 0 2-2v-7"></path><path d="M18.5 2.5a2.121 2.121 0 0 1 3 3L12 15l-4 1 1-4L18.5 2.5z"></path></svg>
            </div>
            <div class="card-delete-btn" onclick="event.stopPropagation(); deleteRecipe(${r.id})" title="Delete" style="position: absolute; top: 6px; left: 6px; width: 24px; height: 24px; background: rgba(239, 68, 68, 0.05); border-radius: 6px; display: flex; align-items: center; justify-content: center; color: var(--danger); transition: all 0.2s; z-index: 5; cursor: pointer;">
                <svg viewBox="0 0 24 24" width="13" height="13" stroke="currentColor" stroke-width="2.5" fill="none"><polyline points="3 6 5 6 21 6"></polyline><path d="M19 6v14a2 2 0 0 1-2 2H7a2 2 0 0 1-2-2V6m3 0V4a2 2 0 0 1 2-2h4a2 2 0 0 1 2 2v2"></path><line x1="10" y1="11" x2="10" y2="17"></line><line x1="14" y1="11" x2="14" y2="17"></line></svg>
            </div>
            <div class="product-img" style="margin-top: 20px;">
                ${(() => {
                    const recipeImage = r.image;
                    if (recipeImage) {
                        if (isImagePath(recipeImage)) {
                            return `<img src="${recipeImage}" class="cat-icon-img" alt="${r.name}">`;
                        } else {
                            return `<span class="emoji-icon">${recipeImage}</span>`;
                        }
                    }
                    return `<span class="emoji-icon">🍲</span>`;
                })()}
            </div>
            <div class="product-info" style="text-align: center; padding: 10px;">
                <div class="product-name">${r.name}</div>
                <div style="font-size: 0.72rem; color: var(--text-muted); margin-top: 4px;">Item No: <span style="color: var(--text-main); font-weight: 600;">${r.itemNo || 'N/A'}</span></div>
                <div style="display:flex; justify-content:space-around; align-items:center; margin-top:8px;">
                    ${shouldShowCost() ? `
                    <div style="display:flex; flex-direction:column; align-items:center;">
                        <span style="font-size:0.7rem; color:var(--text-muted); font-weight:600; text-transform:uppercase;">Cost</span>
                        <span style="color:var(--text); font-weight:700; font-size:0.85rem;">${formatPrice(r.totalCost || 0, 'auto')}</span>
                    </div>
                    ` : ''}
                    <div style="display:flex; flex-direction:column; align-items:center;">
                        <span style="font-size:0.7rem; color:var(--text-muted); font-weight:600; text-transform:uppercase;">Price</span>
                        <span style="color:var(--accent); font-weight:800; font-size:0.9rem;">${formatPrice(r.price)}</span>
                    </div>
                </div>
                <div class="product-stock" style="font-size: 0.75rem; font-weight: 600; color: var(--text-muted); margin-top: 8px;">${r.categoryName ? `Recipe • ${r.categoryName}` : 'Recipe'}</div>
            </div>
        `;
        grid.appendChild(card);
    });
}

function openNewRecipeModal() {
    document.getElementById('recipeModalTitle').innerText = "Create New Recipe";
    document.getElementById('editRecipeId').value = '';
    document.getElementById('recipeItemNo').value = '';
    document.getElementById('recipeName').value = '';
    document.getElementById('recipeCategory').value = '';
    document.getElementById('recipeDesc').value = '';
    document.getElementById('recipePrice').value = '';
    document.getElementById('recipeYieldQty').value = '';
    document.getElementById('recipeYieldUnit').value = '';
    document.getElementById('recipeIngredientsList').innerHTML = '';
    
    selectedRecipeIcon = '🍲';
    updateIconPreview('recipe', '🍲');
    
    addRecipeIngredientRow();
    const totalRow = document.getElementById('recipeIngredientsTotalCostRow');
    if (totalRow) totalRow.style.display = shouldShowCost() ? 'flex' : 'none';
    document.getElementById('recipeModal').classList.remove('hidden');
}

function closeRecipeModal() {
    document.getElementById('recipeModal').classList.add('hidden');
}

function addRecipeIngredientRow(selectedPartId = '', qty = 1, savedUom = '') {
    const list = document.getElementById('recipeIngredientsList');
    if (!list) return;

    const row = document.createElement('div');
    row.className = 'recipe-ingredient-row';
    row.style.display = 'grid';
    const showCost = shouldShowCost();
    row.style.gridTemplateColumns = showCost ? '2fr 80px 100px 90px 90px 30px' : '2fr 80px 100px 30px';
    row.style.gap = '10px';
    row.style.alignItems = 'center';
    row.style.marginBottom = '8px';

    let options = allProducts.map(p => `<option value="${p.id}" ${p.id == selectedPartId ? 'selected' : ''}>${p.name}</option>`).join('');

    let uomOptions = '';
    if (selectedPartId) {
        const product = allProducts.find(p => p.id == selectedPartId);
        if (product) {
            uomOptions = getUomOptionsHtml(product, savedUom);
        }
    }
    if (!uomOptions) {
        uomOptions = getUomOptionsHtml('pcs', savedUom);
    }

    row.innerHTML = `
        <select class="ingredient-select" onchange="onIngredientProductChange(this)" style="width:100%; background:rgba(0,0,0,0.3); color:white; border:2px solid var(--accent); padding:8px; border-radius:6px;">
            <option value="">-- Choose Ingredient --</option>
            ${options}
        </select>
        <input type="number" class="ingredient-qty" oninput="updateRecipeModalCosts()" placeholder="Qty" value="${qty}" min="0.01" step="0.01" style="width:100%; background:rgba(0,0,0,0.3); color:white; border:2px solid var(--accent); padding:8px; border-radius:6px; text-align:center;">
        <select class="ingredient-uom" onchange="updateRecipeModalCosts()" style="width:100%; background:rgba(0,0,0,0.3); color:white; border:2px solid var(--accent); padding:8px; border-radius:6px; font-weight:600; text-align:center; font-size:0.9rem;">
            ${uomOptions}
        </select>
        ${showCost ? `
        <span class="ingredient-unit-cost" style="color:var(--text-muted); text-align:right; font-size:0.85rem; overflow:hidden; text-overflow:ellipsis; white-space:nowrap;">$0.00</span>
        <span class="ingredient-total-cost" style="color:var(--accent); font-weight:bold; text-align:right; font-size:0.95rem;">$0.00</span>
        ` : ''}
        <button onclick="removeRecipeIngredientRow(this)" style="background:none; border:none; color:var(--danger); font-size:1.3rem; cursor:pointer; padding:0; line-height:1;">&times;</button>
    `;
    list.appendChild(row);
    updateRecipeModalCosts();
}

window.onIngredientProductChange = function(selectElem) {
    const row = selectElem.parentElement;
    const uomSelect = row.querySelector('.ingredient-uom');
    if (!uomSelect) return;

    const partId = parseInt(selectElem.value);
    if (partId) {
        const product = allProducts.find(p => p.id === partId);
        if (product) {
            uomSelect.innerHTML = getUomOptionsHtml(product);
        }
    } else {
        uomSelect.innerHTML = getUomOptionsHtml('pcs');
    }
    updateRecipeModalCosts();
};

function getUomOptionsHtml(prodOrUom, selectedUom = '') {
    let baseUom = 'pcs';
    let stockType = 'Piece';
    let bigUnit = '';
    let smallUnit = '';
    if (typeof prodOrUom === 'object' && prodOrUom !== null) {
        baseUom = prodOrUom.unitOfMeasure || 'pcs';
        stockType = prodOrUom.stockType || 'Piece';
        bigUnit = prodOrUom.bigUnit || '';
        smallUnit = prodOrUom.smallUnit || '';
    } else if (typeof prodOrUom === 'string') {
        baseUom = prodOrUom;
    }

    baseUom = baseUom.toLowerCase().trim();
    if (!selectedUom) selectedUom = baseUom;
    selectedUom = selectedUom.toLowerCase().trim();

    if (bigUnit && smallUnit) {
        if (selectedUom === 'pack') selectedUom = bigUnit.toLowerCase();
        const options = [
            { val: bigUnit.toLowerCase(), text: bigUnit },
            { val: smallUnit.toLowerCase(), text: smallUnit }
        ];
        return options.map(o => `<option value="${o.val}" ${o.val === selectedUom.toLowerCase() ? 'selected' : ''}>${o.text}</option>`).join('');
    }

    if (selectedUom.startsWith('gram') || selectedUom === 'g') selectedUom = 'g';
    else if (selectedUom.startsWith('kilo') || selectedUom === 'kg') selectedUom = 'kg';
    else if (selectedUom.startsWith('liter') || selectedUom === 'l') selectedUom = 'l';
    else if (selectedUom === 'ml') selectedUom = 'ml';
    else selectedUom = 'pcs';

    let options = [
        { val: 'pcs', text: 'pcs' },
        { val: 'g', text: 'gram' },
        { val: 'kg', text: 'kg' }
    ];

    if (baseUom.startsWith('l') || baseUom.startsWith('ml')) {
        options = [
            { val: 'ml', text: 'ml' },
            { val: 'l', text: 'l' },
            { val: 'pcs', text: 'pcs' }
        ];
    } else if (stockType === 'Pack') {
        options = [
            { val: 'pcs', text: 'pcs' },
            { val: 'g', text: 'gram' }
        ];
    }

    return options.map(o => `<option value="${o.val}" ${o.val === selectedUom ? 'selected' : ''}>${o.text}</option>`).join('');
}

function removeRecipeIngredientRow(btn) {
    const row = btn.parentElement;
    row.remove();
    updateRecipeModalCosts();
}

window.updateRecipeModalCosts = function() {
    let totalCost = 0;
    const rows = document.querySelectorAll('.recipe-ingredient-row');
    rows.forEach(row => {
        const select = row.querySelector('.ingredient-select');
        const qtyInput = row.querySelector('.ingredient-qty');
        const uomSelect = row.querySelector('.ingredient-uom');
        const unitCostSpan = row.querySelector('.ingredient-unit-cost');
        const totalCostSpan = row.querySelector('.ingredient-total-cost');
        
        if (!select || !qtyInput || !uomSelect) return;
        
        const partId = parseInt(select.value);
        const qty = parseFloat(qtyInput.value) || 0;
        const chosenUom = uomSelect.value.toLowerCase().trim();
        
        if (partId) {
            const product = allProducts.find(p => p.id === partId);
            if (product) {
                const baseUom = (product.unitOfMeasure || 'pcs').toLowerCase().trim();
                const baseCost = product.packPrice || product.purchasePrice || 0;
                let convertedCost = baseCost;

                const bigUnit = (product.bigUnit || '').toLowerCase().trim();
                const smallUnit = (product.smallUnit || '').toLowerCase().trim();

                if (bigUnit && smallUnit) {
                    const convVal = parseFloat(product.conversionValue) || 1.0;
                    const pSize = parseFloat(product.packSize) || 1.0;
                    const big = bigUnit;
                    const small = smallUnit;
                    const uom = chosenUom;

                    const costPerBigUnit = (big === "pack" || big === "package") ? baseCost : (baseCost / pSize);
                    const costPerSmallUnit = costPerBigUnit / convVal;

                    if (big === small) {
                        if (uom === "pack") {
                            convertedCost = baseCost;
                        } else {
                            convertedCost = costPerBigUnit;
                        }
                    } else if (uom === small) {
                        convertedCost = costPerSmallUnit;
                    } else if (uom === big) {
                        convertedCost = costPerBigUnit;
                    } else if (uom === "pack") {
                        convertedCost = baseCost;
                    } else {
                        convertedCost = costPerSmallUnit;
                    }
                } else if (product.stockType === 'Pack') {
                    const packItems = product.packItemsNumber || 1;
                    const itemCost = baseCost / packItems;
                    if (chosenUom === 'pcs') {
                        convertedCost = itemCost;
                    } else if (chosenUom === 'g') {
                        if (baseUom.startsWith('kilo') || baseUom === 'kg') {
                            convertedCost = itemCost / 1000;
                        } else {
                            convertedCost = itemCost;
                        }
                    } else if (chosenUom === 'kg') {
                        if (baseUom.startsWith('gram') || baseUom === 'g') {
                            convertedCost = itemCost * 1000;
                        } else {
                            convertedCost = itemCost;
                        }
                    } else {
                        convertedCost = itemCost;
                    }
                } else {
                    if ((baseUom.startsWith('kilo') || baseUom === 'kg') && chosenUom === 'g') {
                        convertedCost = baseCost / 1000;
                    } else if ((baseUom.startsWith('gram') || baseUom === 'g') && chosenUom === 'kg') {
                        convertedCost = baseCost * 1000;
                    } else if ((baseUom.startsWith('liter') || baseUom === 'l') && chosenUom === 'ml') {
                        convertedCost = baseCost / 1000;
                    } else if (baseUom === 'ml' && chosenUom === 'l') {
                        convertedCost = baseCost * 1000;
                    }
                }

                const rowCost = convertedCost * qty;
                totalCost += rowCost;
                
                if (unitCostSpan) unitCostSpan.textContent = `$${formatDynamicNumber(convertedCost)}/${chosenUom}`;
                if (totalCostSpan) totalCostSpan.textContent = `$${formatDynamicNumber(rowCost)}`;
            }
        } else {
            if (unitCostSpan) unitCostSpan.textContent = '$0.00';
            if (totalCostSpan) totalCostSpan.textContent = '$0.00';
        }
    });
    
    const totalDisplay = document.getElementById('recipeIngredientsTotalCost');
    if (totalDisplay) {
        totalDisplay.textContent = `$${formatDynamicNumber(totalCost)}`;
    }
};

async function saveRecipe() {
    const id = document.getElementById('editRecipeId').value;
    const itemNo = document.getElementById('recipeItemNo').value;
    const name = document.getElementById('recipeName').value;
    const categoryName = document.getElementById('recipeCategory').value;
    const desc = document.getElementById('recipeDesc').value;
    const price = parseFloat(document.getElementById('recipePrice').value);
    const yieldQty = parseFloat(document.getElementById('recipeYieldQty').value) || 1.0;
    const yieldUnit = document.getElementById('recipeYieldUnit').value || '';

    if (!name || isNaN(price)) {
        showToast("Please fill Recipe Name and Selling Price", "error");
        return;
    }

    const ingredientRows = document.querySelectorAll('.recipe-ingredient-row');
    const ingredients = [];
    ingredientRows.forEach(row => {
        const partId = row.querySelector('.ingredient-select').value;
        const qty = parseFloat(row.querySelector('.ingredient-qty').value);
        const uomSelect = row.querySelector('.ingredient-uom');
        const uom = uomSelect ? uomSelect.value : 'pcs';
        if (partId && qty > 0) {
            ingredients.push({ partId: parseInt(partId), qty, unitOfMeasure: uom });
        }
    });

    if (ingredients.length === 0) {
        showToast("Add at least one valid ingredient", "error");
        return;
    }

    const payload = { id: id ? parseInt(id) : null, name, itemNo, categoryName, description: desc, price, yieldQuantity: yieldQty, yieldUnit, ingredients, image: selectedRecipeIcon };

    try {
        const res = await fetch(`${API_BASE}/api/recipes`, {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify(payload)
        });

        if (res.ok) {
            showToast(id ? "Recipe updated!" : "Recipe created!", "success");
            closeRecipeModal();
            await fetchRecipes();
            loadRecipesTab();
        } else {
            showToast("Failed to save recipe", "error");
        }
    } catch (e) { showToast("Connection error", "error"); }
}

function deleteRecipe(id) {
    showDeleteConfirm(
        "Delete Recipe",
        "Are you sure you want to delete this recipe? Components will remain in inventory.",
        async () => {
            try {
                const res = await fetch(`${API_BASE}/api/recipes/${id}`, { method: 'DELETE' });
                if (res.ok) {
                    showToast("Recipe deleted successfully", "success");
                    await fetchRecipes();
                    loadRecipesTab();
                } else {
                    showToast("Failed to delete recipe", "error");
                }
            } catch (e) { showToast("Connection error", "error"); }
        }
    );
}

function editRecipe(id) {
    const r = allRecipes.find(x => x.id === id);
    if (!r) return;

    document.getElementById('recipeModalTitle').innerText = "Edit Recipe";
    document.getElementById('editRecipeId').value = r.id;
    document.getElementById('recipeItemNo').value = r.itemNo || '';
    document.getElementById('recipeName').value = r.name;
    document.getElementById('recipeCategory').value = r.categoryName || '';
    document.getElementById('recipeDesc').value = r.description || '';
    document.getElementById('recipePrice').value = r.price;
    document.getElementById('recipeYieldQty').value = r.yieldQuantity || '';
    document.getElementById('recipeYieldUnit').value = r.yieldUnit || '';

    selectedRecipeIcon = r.image || '🍲';
    updateIconPreview('recipe', selectedRecipeIcon);

    const list = document.getElementById('recipeIngredientsList');
    list.innerHTML = '';
    
    console.log("=== INGREDIENTS COMPARISON LOG ===");
    console.log("Recipe ID:", r.id, "| Recipe Name:", r.name);
    if (r.parts && r.parts.length > 0) {
        r.parts.forEach((p, idx) => {
            const product = allProducts.find(prod => prod.id === p.partId);
            const baseUnit = product ? (product.unitOfMeasure || 'pcs') : 'pcs';
            const bigUnit = product ? (product.bigUnit || '') : '';
            const smallUnit = product ? (product.smallUnit || '') : '';
            const conversionVal = product ? (parseFloat(product.conversionValue) || 1.0) : 1.0;
            const packSizeVal = product ? (parseFloat(product.packSize) || 1.0) : 1.0;
            
            // Calculate base quantity
            let baseQuantity = p.qty;
            let baseUnitId = baseUnit;
            if (bigUnit && smallUnit) {
                baseUnitId = smallUnit;
                if (p.unitOfMeasure && p.unitOfMeasure.toLowerCase().trim() === bigUnit.toLowerCase().trim()) {
                    baseQuantity = p.qty * conversionVal;
                }
            } else if (product && product.stockType === 'Pack') {
                const packItems = product.packItemsNumber || 1;
                if (p.unitOfMeasure && p.unitOfMeasure.toLowerCase().trim() === 'pcs') {
                    baseQuantity = p.qty;
                } else {
                    baseQuantity = p.qty * packItems;
                }
            }

            console.log(`Ingredient #${idx + 1}: ${p.partName || 'Unknown'}`);
            console.log("  quantity:", p.qty);
            console.log("  selected unit:", p.unitOfMeasure || 'pcs');
            console.log("  base quantity:", baseQuantity);
            console.log("  conversion factor:", conversionVal);
            console.log("  cost:", p.totalCost);
            console.log("  unit id:", p.partId);
            console.log("  base unit id:", baseUnitId);
        });
    } else {
        console.log("No ingredients in this recipe.");
    }
    console.log("==================================");

    if (r.parts && r.parts.length > 0) {
        r.parts.forEach(p => {
            addRecipeIngredientRow(p.partId, p.qty, p.unitOfMeasure);
        });
    } else {
        addRecipeIngredientRow();
    }
    const totalRow = document.getElementById('recipeIngredientsTotalCostRow');
    if (totalRow) totalRow.style.display = shouldShowCost() ? 'flex' : 'none';
    document.getElementById('recipeModal').classList.remove('hidden');
}

let pendingImportItems = [];
let pendingImportType = 'ingredients'; // 'ingredients' or 'recipes'

function handleImportFileSelect(e) {
    const file = e.target.files[0];
    if (!file) return;

    const reader = new FileReader();
    const isExcel = /\.xlsx$/i.test(file.name) || /\.xls$/i.test(file.name);

    if (isExcel) {
        reader.onload = function(evt) {
            try {
                const data = new Uint8Array(evt.target.result);
                const workbook = XLSX.read(data, { type: 'array' });
                const firstSheetName = workbook.SheetNames[0];
                const worksheet = workbook.Sheets[firstSheetName];
                const csv = XLSX.utils.sheet_to_csv(worksheet);
                parseImportFile(csv, false);
            } catch (err) {
                console.error(err);
                showToast("Failed to parse Excel file: " + err.message, "error");
            }
        };
        reader.readAsArrayBuffer(file);
    } else {
        reader.onload = function(evt) {
            const content = evt.target.result;
            parseImportFile(content, /\.tsv$/i.test(file.name) || content.includes('\t'));
        };
        reader.readAsText(file);
    }
}

function parseImportFile(text, isTsv) {
    const lines = text.split('\n').map(l => l.trim()).filter(l => l.length > 0);
    if (lines.length <= 1) {
        showToast("Import file is empty", "warn");
        return;
    }

    let separator = ',';
    if (isTsv) {
        separator = '\t';
    } else {
        const commaCount = (lines[0].match(/,/g) || []).length;
        const semiCount = (lines[0].match(/;/g) || []).length;
        const tabCount = (lines[0].match(/\t/g) || []).length;
        if (semiCount > commaCount && semiCount > tabCount) {
            separator = ';';
        } else if (tabCount > commaCount && tabCount > semiCount) {
            separator = '\t';
        } else {
            separator = ',';
        }
    }
    const headers = lines[0].split(separator).map(h => h.replace(/"/g, '').trim());
    
    const findColIdx = (synonyms) => {
        for (const syn of synonyms) {
            const cleanSyn = syn.toLowerCase().replace(/[\s_-]/g, '');
            const idx = headers.findIndex(h => {
                const cleanHeader = h.toLowerCase().replace(/[\s_-]/g, '');
                return cleanHeader === cleanSyn;
            });
            if (idx !== -1) return idx;
        }
        return -1;
    };

    const recipeNameIdx = findColIdx(['recipe name', 'recipe_name', 'recipe']);

    if (recipeNameIdx !== -1) {
        // --- RECIPE CSV IMPORT ---
        pendingImportType = 'recipes';
        
        const catIdx = findColIdx(['category', 'category name', 'categoryname', 'recipe category']);
        const priceIdx = findColIdx(['selling price', 'price', 'recipe price', 'sellingprice']);
        const descIdx = findColIdx(['description', 'desc', 'notes', 'note']);
        const itemNoIdx = findColIdx(['item no', 'itemno', 'no.', 'recipe item no']);
        const ingNameIdx = findColIdx(['ingredient name', 'ingredientname', 'ingredient', 'part name', 'partname', 'part']);
        const ingQtyIdx = findColIdx(['ingredient quantity', 'ingredientquantity', 'quantity', 'qty', 'ingredient qty', 'ingredientqty', 'ingredient_quantity']);
        const ingUomIdx = findColIdx(['measurement unit', 'measurementunit', 'unit of measure', 'unitofmeasure', 'uom', 'unit']);

        const recipesMap = {};
        for (let i = 1; i < lines.length; i++) {
            const cols = lines[i].split(separator).map(c => c.replace(/"/g, '').trim());
            if (cols.length < headers.length) continue;

            const rName = cols[recipeNameIdx];
            if (!rName) continue;

            if (!recipesMap[rName]) {
                recipesMap[rName] = {
                    name: rName,
                    categoryName: catIdx !== -1 ? cols[catIdx] : 'General',
                    price: priceIdx !== -1 ? parseFloat(cols[priceIdx]) || 0.0 : 0.0,
                    description: descIdx !== -1 ? cols[descIdx] : '',
                    itemNo: itemNoIdx !== -1 ? cols[itemNoIdx] : '',
                    ingredients: []
                };
            }

            const ingName = ingNameIdx !== -1 ? cols[ingNameIdx] : '';
            const ingQty = ingQtyIdx !== -1 ? parseFloat(cols[ingQtyIdx]) || 1.0 : 1.0;
            const ingUom = ingUomIdx !== -1 ? cols[ingUomIdx] : '';

            if (ingName) {
                recipesMap[rName].ingredients.push({
                    name: ingName,
                    qty: ingQty,
                    unitOfMeasure: ingUom
                });
            }
        }

        pendingImportItems = Object.values(recipesMap);

        if (pendingImportItems.length === 0) {
            showToast("No valid recipes found in file", "warn");
            return;
        }

        // Show recipe import preview
        if (document.getElementById('importPreviewCount')) {
            document.getElementById('importPreviewCount').innerText = pendingImportItems.length + " recipes";
        }
        const previewList = document.getElementById('importPreviewList');
        if (previewList) {
            previewList.innerHTML = pendingImportItems.map(recipe => {
                const ingList = recipe.ingredients.map(ing => `${ing.name} (${ing.qty} ${ing.unitOfMeasure || ''})`).join(', ');
                return `
                    <div style="border-bottom:1px solid rgba(255,255,255,0.05); padding:8px 0;">
                        <div style="display:flex; justify-content:space-between; align-items:center;">
                            <b style="color:var(--text-main);">${recipe.itemNo ? '[' + recipe.itemNo + '] ' : ''}${recipe.name}</b>
                            <span style="color:var(--accent); font-weight:bold;">$${recipe.price.toFixed(2)}</span>
                        </div>
                        <div style="font-size:0.75rem; color:var(--text-muted); margin-top:2px;">Category: ${recipe.categoryName}</div>
                        <div style="font-size:0.75rem; color:var(--text-muted); margin-top:2px; word-break:break-all;">Ingredients: ${ingList || 'None'}</div>
                    </div>
                `;
            }).join('');
        }

        const importPreviewArea = document.getElementById('importPreviewArea');
        if (importPreviewArea) importPreviewArea.classList.remove('hidden');

        // Render recipe preview in modal preview
        const modalPreviewList = document.getElementById('modalImportPreviewList');
        if (modalPreviewList) {
            modalPreviewList.innerHTML = pendingImportItems.map(recipe => {
                const ingList = recipe.ingredients.map(ing => `${ing.name} (${ing.qty} ${ing.unitOfMeasure || ''})`).join(', ');
                return `
                    <div style="border-bottom:1px solid rgba(255,255,255,0.05); padding:8px 0; text-align:left;">
                        <div style="display:flex; justify-content:space-between; align-items:center;">
                            <b style="color:white;">${recipe.itemNo ? '[' + recipe.itemNo + '] ' : ''}${recipe.name}</b>
                            <span style="color:var(--accent); font-weight:bold;">$${recipe.price.toFixed(2)}</span>
                        </div>
                        <div style="font-size:0.75rem; color:var(--text-muted); margin-top:2px;">Category: ${recipe.categoryName}</div>
                        <div style="font-size:0.75rem; color:var(--text-muted); margin-top:2px; word-break:break-all; color:rgba(255,255,255,0.7);">Ingredients: ${ingList || 'None'}</div>
                    </div>
                `;
            }).join('');
            document.getElementById('modalImportTitle').innerText = `Confirm Bulk Import (${pendingImportItems.length} recipes)`;
            document.getElementById('importPreviewModal').classList.remove('hidden');
        }

        showToast(`Parsed ${pendingImportItems.length} recipes from file`, "info");

    } else {
        // --- INGREDIENT/PART CSV IMPORT (Legacy) ---
        pendingImportType = 'ingredients';

        const nameIdx = findColIdx(['ingredient name', 'ingredientname', 'ingredient', 'name', 'part name', 'partname']);
        const catIdx = findColIdx(['category', 'category name', 'categoryname']);
        const bigUnitIdx = findColIdx(['big unit', 'bigunit', 'big_unit', 'big uom', 'base unit', 'baseunit', 'base_unit']);
        const smallUnitIdx = findColIdx(['small unit', 'smallunit', 'small_unit', 'small uom', 'sub unit', 'subunit', 'sub_unit']);
        const conversionIdx = findColIdx(['conversion value', 'conversionvalue', 'conversion', 'conversion_value', 'conversion factor']);
        const packQtyIdx = findColIdx(['pack quantity', 'packquantity', 'pack size', 'packsize', 'pack_size', 'pack quan', 'packquan', 'pack qty', 'packqty']);
        const packPriceIdx = findColIdx(['pack price', 'packprice', 'pack_price', 'purchase price', 'purchase_price', 'cost', 'pack cost']);
        const priceIdx = findColIdx(['selling price', 'price', 'unit price', 'sellingprice', 'unitprice']);
        const stockIdx = findColIdx(['current stock', 'stock', 'quantity', 'qty', 'quantity_in_stock', 'currentstock']);
        const minStockIdx = findColIdx(['minimum stock', 'min stock', 'minimumstock', 'minstock', 'minimum_stock_level']);
        const barcodeIdx = findColIdx(['barcode']);
        const skuIdx = findColIdx(['sku', 'part number', 'partnumber', 'part_number']);
        const descIdx = findColIdx(['description', 'desc', 'notes', 'note']);
        const itemNoIdx = findColIdx(['item no', 'itemno', 'no.']);

        if (nameIdx === -1) {
            showToast("Invalid file format. 'Name' or 'Recipe Name' column is required.", "error");
            return;
        }

        pendingImportItems = [];
        for (let i = 1; i < lines.length; i++) {
            const cols = lines[i].split(separator).map(c => c.replace(/"/g, '').trim());
            if (cols.length < headers.length) continue;

            const pPrice = packPriceIdx !== -1 ? parseFloat(cols[packPriceIdx]) || 0.00 : (priceIdx !== -1 ? parseFloat(cols[priceIdx]) || 0.00 : 0.00);

            pendingImportItems.push({
                itemNo: itemNoIdx !== -1 ? cols[itemNoIdx] : '',
                name: cols[nameIdx] || '',
                category: catIdx !== -1 ? cols[catIdx] : 'General',
                price: priceIdx !== -1 ? parseFloat(cols[priceIdx]) || pPrice : pPrice,
                stock: stockIdx !== -1 ? parseFloat(cols[stockIdx]) || 0 : 0,
                barcode: barcodeIdx !== -1 ? cols[barcodeIdx] : '',
                sku: skuIdx !== -1 ? cols[skuIdx] : '',
                description: descIdx !== -1 ? cols[descIdx] : '',
                bigUnit: bigUnitIdx !== -1 ? cols[bigUnitIdx] : '',
                smallUnit: smallUnitIdx !== -1 ? cols[smallUnitIdx] : '',
                conversionValue: conversionIdx !== -1 ? parseFloat(cols[conversionIdx]) || 1.0 : 1.0,
                packSize: packQtyIdx !== -1 ? parseFloat(cols[packQtyIdx]) || 1.0 : 1.0,
                packPrice: pPrice,
                minStock: minStockIdx !== -1 ? parseInt(cols[minStockIdx]) || 5 : 5
            });
        }

        // Show preview in tab preview
        if (document.getElementById('importPreviewCount')) {
            document.getElementById('importPreviewCount').innerText = pendingImportItems.length + " ingredients";
        }
        const previewList = document.getElementById('importPreviewList');
        if (previewList) {
            previewList.innerHTML = pendingImportItems.map(item => `
                <div style="border-bottom:1px solid rgba(255,255,255,0.05); padding:8px 0; display:grid; grid-template-columns:1.5fr 1fr 1fr 1fr; gap:10px;">
                    <b style="color:var(--text-main); overflow:hidden; text-overflow:ellipsis; white-space:nowrap;">${item.itemNo ? '[' + item.itemNo + '] ' : ''}${item.name}</b>
                    <span style="color:var(--text-muted);">${item.category}</span>
                    <span style="color:var(--accent); text-align:right;">$${item.price.toFixed(2)}</span>
                    <span style="color:${item.stock === 0 ? 'var(--danger)' : 'var(--text-main)'}; text-align:right;">Qty: ${item.stock}</span>
                </div>
            `).join('');
        }

        const importPreviewArea = document.getElementById('importPreviewArea');
        if (importPreviewArea) importPreviewArea.classList.remove('hidden');

        // Render preview in modal preview
        const modalPreviewList = document.getElementById('modalImportPreviewList');
        if (modalPreviewList) {
            modalPreviewList.innerHTML = pendingImportItems.map(item => `
                <div style="border-bottom:1px solid rgba(255,255,255,0.05); padding:8px 0; display:grid; grid-template-columns:1.5fr 1.2fr 1fr 1fr; gap:10px; align-items:center; text-align:left;">
                    <b style="color:white; overflow:hidden; text-overflow:ellipsis; white-space:nowrap;">${item.itemNo ? '[' + item.itemNo + '] ' : ''}${item.name}</b>
                    <span style="color:var(--text-muted);">${item.category}</span>
                    <span style="color:var(--accent); text-align:right; font-weight:bold;">$${item.price.toFixed(2)}</span>
                    <span style="color:${item.stock === 0 ? 'var(--danger)' : 'white'}; text-align:right;">Qty: ${item.stock}</span>
                </div>
            `).join('');
            document.getElementById('modalImportTitle').innerText = `Confirm Bulk Import (${pendingImportItems.length} ingredients)`;
            document.getElementById('importPreviewModal').classList.remove('hidden');
        }

        showToast(`Parsed ${pendingImportItems.length} items from file`, "info");
    }
}

function clearImportPreview() {
    pendingImportItems = [];
    if (document.getElementById('importFile')) document.getElementById('importFile').value = '';
    if (document.getElementById('recipeImportFile')) document.getElementById('recipeImportFile').value = '';
    if (document.getElementById('salesImportFile')) document.getElementById('salesImportFile').value = '';
    if (document.getElementById('importPreviewArea')) document.getElementById('importPreviewArea').classList.add('hidden');
    if (document.getElementById('importPreviewList')) document.getElementById('importPreviewList').innerHTML = '';
}

async function confirmImport() {
    if (pendingImportItems.length === 0) return;
    try {
        const endpoint = pendingImportType === 'recipes' ? 'api/import-recipes' : 'api/import-items';
        const payload = pendingImportType === 'recipes' ? { recipes: pendingImportItems } : { items: pendingImportItems };

        const res = await fetch(`${API_BASE}/${endpoint}`, {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify(payload)
        });
        if (res.ok) {
            const result = await res.json();
            if (result.error) {
                showToast(`Failed to import: ${result.error}`, "error");
            } else {
                showToast(`Import Success! Imported: ${result.imported}, Skipped: ${result.skipped}`, "success");
                clearImportPreview();
                await initApp();
            }
        } else {
            showToast("Failed to process import on server", "error");
        }
    } catch (e) { showToast("Connection error", "error"); }
}

async function sendFileToBackend(filename, csvContent, base64Content) {
    if (window.AndroidBridge || API_BASE !== '') {
        const res = await fetch(`${API_BASE}/api/export-csv`, {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify({ filename, csvContent, base64Content })
        });
        if (res.ok) {
            const data = await res.json();
            showToast(`Exported successfully to ${data.path}!`, "success");
        } else {
            showToast("Export failed on device", "error");
        }
    } else {
        // Fallback for browser download
        let blob;
        if (base64Content) {
            const binaryString = window.atob(base64Content);
            const bytes = new Uint8Array(binaryString.length);
            for (let i = 0; i < binaryString.length; i++) {
                bytes[i] = binaryString.charCodeAt(i);
            }
            blob = new Blob([bytes], { type: 'application/vnd.openxmlformats-officedocument.spreadsheetml.sheet' });
        } else {
            blob = new Blob([csvContent], { type: 'text/csv;charset=utf-8;' });
        }
        const link = document.createElement("a");
        const url = URL.createObjectURL(blob);
        link.setAttribute("href", url);
        link.setAttribute("download", filename);
        link.style.visibility = 'hidden';
        document.body.appendChild(link);
        link.click();
        document.body.removeChild(link);
        showToast("Downloaded successfully!", "success");
    }
}

window.exportInventory = async function(format) {
    try {
        const headers = ["Item No", "Ingredient Name", "Category", "Base Unit", "Sub Unit", "Conversion Value", "Cost", "Current Stock", "Minimum Stock"];
        const rows = allProducts.map(p => {
            const conv = parseFloat(p.conversionValue) || 1.0;
            const cost = parseFloat(p.packPrice || p.purchasePrice || p.price || 0.00);
            const currentStock = p.stock || 0;
            const minStock = p.minStock || 0;

            return [
                p.itemNo || '',
                p.name || '',
                p.category || 'General',
                p.bigUnit || '',
                p.smallUnit || '',
                conv,
                cost,
                currentStock,
                minStock
            ];
        });

        if (format === 'excel') {
            const worksheet = XLSX.utils.aoa_to_sheet([headers, ...rows]);
            const workbook = XLSX.utils.book_new();
            XLSX.utils.book_append_sheet(workbook, worksheet, "Inventory");
            const base64Content = XLSX.write(workbook, { bookType: 'xlsx', type: 'base64' });
            const filename = `inventory_${new Date().toISOString().slice(0,10)}.xlsx`;

            await sendFileToBackend(filename, null, base64Content);
        } else {
            // CSV Format
            const csvRows = rows.map(r => r.map(val => {
                if (typeof val === 'string') {
                    return `"${val.replace(/"/g, '""')}"`;
                }
                return val;
            }));
            const csvContent = [headers.join(','), ...csvRows.map(r => r.join(','))].join('\n');
            const filename = `inventory_${new Date().toISOString().slice(0,10)}.csv`;

            await sendFileToBackend(filename, csvContent, null);
        }
    } catch (e) {
        showToast("Error exporting inventory: " + e.message, "error");
    }
};

function downloadCsvTemplate() {
    const csvContent = "Item No,Ingredient,Category,Big Unit,Small Unit,Conversion,Pack Quan,Pack Price,Current Stock,Minimum Stock\n1001,BBQ Sauce,Food,L,ml,1000,1,6.99,5000,500\n1002,Basil,Food,Kg,g,1000,1,7.99,5000,500\n1003,Beef,Food,Kg,g,1000,1,8.99,5000,500\n1004,Beef Patty,Food,Kg,g,1000,1,9.99,5000,500\n1005,Bell Pepper,Food,Kg,g,1000,1,10.99,5000,500\n1006,Biscuits,Food,Kg,g,1000,1,11.99,5000,500\n1007,Bread,Food,Pack,Piece,10,10,12.99,5000,500\n";
    
    // In WebView, trigger download by writing to Downloads folder
    fetch(`${API_BASE}/api/export-csv`, {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({ filename: "Import_Template.csv", csvContent })
    })
    .then(res => {
        if (res.ok) showToast("Template downloaded to Downloads folder", "success");
        else showToast("Failed to download template", "error");
    })
    .catch(() => showToast("Connection error", "error"));
}

// ─── RECIPE IMPORT / EXPORT ──────────────────────────────────────────
let pendingRecipeImportItems = [];

function handleRecipeImportFileSelect(e) {
    const file = e.target.files[0];
    if (!file) return;

    const reader = new FileReader();
    const isExcel = file.name.endsWith('.xlsx') || file.name.endsWith('.xls');

    if (isExcel) {
        reader.onload = function(evt) {
            try {
                const data = new Uint8Array(evt.target.result);
                const workbook = XLSX.read(data, { type: 'array' });
                const firstSheetName = workbook.SheetNames[0];
                const worksheet = workbook.Sheets[firstSheetName];
                
                // Read sheet to array of arrays
                const rows = XLSX.utils.sheet_to_json(worksheet, { header: 1, defval: "" });
                parseRecipeImportRows(rows);
            } catch (err) {
                console.error(err);
                showToast("Failed to parse Excel file: " + err.message, "error");
            }
        };
        reader.readAsArrayBuffer(file);
    } else {
        reader.onload = function(evt) {
            const content = evt.target.result;
            let separator = ',';
            if (file.name.endsWith('.tsv')) {
                separator = '\t';
            } else {
                const headerLine = content.split('\n')[0] || '';
                const commaCount = (headerLine.match(/,/g) || []).length;
                const semiCount = (headerLine.match(/;/g) || []).length;
                const tabCount = (headerLine.match(/\t/g) || []).length;
                if (semiCount > commaCount && semiCount > tabCount) {
                    separator = ';';
                } else if (tabCount > commaCount && tabCount > semiCount) {
                    separator = '\t';
                } else {
                    separator = ',';
                }
            }
            
            // Parse CSV/TSV into an array of arrays
            const lines = content.split('\n').map(l => l.trim()).filter(l => l.length > 0);
            const rows = lines.map(line => parseCsvLine(line, separator));
            parseRecipeImportRows(rows);
        };
        reader.readAsText(file);
    }
}

function parseCsvLine(text, separator) {
    const result = [];
    let curVal = "";
    let inQuotes = false;
    for (let i = 0; i < text.length; i++) {
        const char = text[i];
        if (char === '"') {
            inQuotes = !inQuotes;
        } else if (char === separator && !inQuotes) {
            result.push(curVal.trim());
            curVal = "";
        } else {
            curVal += char;
        }
    }
    result.push(curVal.trim());
    return result;
}

function parseRecipeImportRows(rows) {
    if (rows.length <= 1) {
        showToast("Import file is empty", "warn");
        return;
    }

    const headers = rows[0].map(h => (h || '').toString().trim());

    const findColIdx = (synonyms) => {
        for (const syn of synonyms) {
            const cleanSyn = syn.toLowerCase().replace(/[\s_-]/g, '');
            const idx = headers.findIndex(h => {
                const cleanHeader = h.toLowerCase().replace(/[\s_-]/g, '');
                return cleanHeader === cleanSyn;
            });
            if (idx !== -1) return idx;
        }
        return -1;
    };

    const recipeNameIdx = findColIdx(['recipe name', 'recipe_name', 'recipe']);
    if (recipeNameIdx === -1) {
        showToast("Invalid format. 'Recipe Name' column is required.", "error");
        return;
    }

    const catIdx = findColIdx(['category', 'category name', 'categoryname', 'recipe category']);
    const priceIdx = findColIdx(['selling price', 'price', 'recipe price', 'sellingprice']);
    const descIdx = findColIdx(['description', 'desc', 'notes', 'note']);
    const itemNoIdx = findColIdx(['item no', 'itemno', 'no.', 'recipe item no']);

    // Find all ingredient / part columns to support duplicate headers
    let ingredientIndices = [];
    headers.forEach((h, idx) => {
        const clean = h.toLowerCase().replace(/[\s_-]/g, '');
        if (clean === 'ingredient' || clean === 'part') {
            ingredientIndices.push(idx);
        }
    });

    let ingNameIdx = -1;
    let ingQtyIdx = -1;

    if (ingredientIndices.length >= 2) {
        ingNameIdx = ingredientIndices[0];
        ingQtyIdx = ingredientIndices[1];
    } else if (ingredientIndices.length === 1) {
        ingNameIdx = ingredientIndices[0];
    }

    if (ingNameIdx === -1) {
        ingNameIdx = findColIdx(['ingredient name', 'ingredientname', 'part name', 'partname', 'part', 'ingredient']);
    }
    if (ingQtyIdx === -1) {
        ingQtyIdx = findColIdx(['ingredient quantity', 'ingredientquantity', 'quantity', 'qty', 'ingredient qty', 'ingredientqty', 'ingredient_quantity', 'quantities', 'amount']);
    }

    const ingUomIdx = findColIdx(['measurement unit', 'measurementunit', 'unit of measure', 'unitofmeasure', 'uom', 'unit']);

    const recipesMap = {};
    for (let i = 1; i < rows.length; i++) {
        const cols = rows[i].map(c => (c === undefined || c === null ? "" : c).toString().trim());
        if (cols.length === 0 || (cols.length === 1 && !cols[0])) continue;

        const rName = cols[recipeNameIdx];
        if (!rName) continue;

        if (!recipesMap[rName]) {
            recipesMap[rName] = {
                name: rName,
                categoryName: catIdx !== -1 ? cols[catIdx] : 'General',
                price: priceIdx !== -1 ? parseFloat(cols[priceIdx]) || 0.0 : 0.0,
                description: descIdx !== -1 ? cols[descIdx] : '',
                itemNo: itemNoIdx !== -1 ? cols[itemNoIdx] : '',
                ingredients: []
            };
        }

        const ingName = ingNameIdx !== -1 ? cols[ingNameIdx] : '';
        const ingQtyVal = ingQtyIdx !== -1 ? cols[ingQtyIdx] : '';
        const ingQty = parseFloat(ingQtyVal) || 1.0;
        const ingUom = ingUomIdx !== -1 ? cols[ingUomIdx] : '';

        if (ingName) {
            recipesMap[rName].ingredients.push({
                name: ingName,
                qty: ingQty,
                unitOfMeasure: ingUom
            });
        }
    }

    pendingRecipeImportItems = Object.values(recipesMap);
    pendingImportItems = pendingRecipeImportItems; // Set shared items for modal confirmation
    pendingImportType = 'recipes'; // Set shared type for modal confirmation

    if (pendingRecipeImportItems.length === 0) {
        showToast("No valid recipes found in file", "warn");
        return;
    }

    // Show recipe import preview in the inline preview area (if visible)
    if (document.getElementById('recipeImportPreviewCount')) {
        document.getElementById('recipeImportPreviewCount').innerText = pendingRecipeImportItems.length;
    }
    const previewList = document.getElementById('recipeImportPreviewList');
    if (previewList) {
        previewList.innerHTML = pendingRecipeImportItems.map(recipe => {
            const ingList = recipe.ingredients.map(ing => `${ing.name} (${ing.qty} ${ing.unitOfMeasure || ''})`).join(', ');
            return `
                <div style="border-bottom:1px solid rgba(255,255,255,0.05); padding:8px 0;">
                    <div style="display:flex; justify-content:space-between; align-items:center;">
                        <b style="color:var(--text-main);">${recipe.itemNo ? '[' + recipe.itemNo + '] ' : ''}${recipe.name}</b>
                        <span style="color:var(--accent); font-weight:bold;">$${recipe.price.toFixed(2)}</span>
                    </div>
                    <div style="font-size:0.75rem; color:var(--text-muted); margin-top:2px;">Category: ${recipe.categoryName}</div>
                    <div style="font-size:0.75rem; color:var(--text-muted); margin-top:2px; word-break:break-all;">Ingredients: ${ingList || 'None'}</div>
                </div>
            `;
        }).join('');
    }
    const recipeImportPreviewArea = document.getElementById('recipeImportPreviewArea');
    if (recipeImportPreviewArea) recipeImportPreviewArea.classList.remove('hidden');

    // Show shared modal preview for immediate confirmation from the Recipes tab
    const modalPreviewList = document.getElementById('modalImportPreviewList');
    if (modalPreviewList) {
        modalPreviewList.innerHTML = pendingRecipeImportItems.map(recipe => {
            const ingList = recipe.ingredients.map(ing => `${ing.name} (${ing.qty} ${ing.unitOfMeasure || ''})`).join(', ');
            return `
                <div style="border-bottom:1px solid rgba(255,255,255,0.05); padding:8px 0; text-align:left;">
                    <div style="display:flex; justify-content:space-between; align-items:center;">
                        <b style="color:white;">${recipe.itemNo ? '[' + recipe.itemNo + '] ' : ''}${recipe.name}</b>
                        <span style="color:var(--accent); font-weight:bold;">$${recipe.price.toFixed(2)}</span>
                    </div>
                    <div style="font-size:0.75rem; color:var(--text-muted); margin-top:2px;">Category: ${recipe.categoryName}</div>
                    <div style="font-size:0.75rem; color:var(--text-muted); margin-top:2px; word-break:break-all; color:rgba(255,255,255,0.7);">Ingredients: ${ingList || 'None'}</div>
                </div>
            `;
        }).join('');
        document.getElementById('modalImportTitle').innerText = `Confirm Bulk Import (${pendingRecipeImportItems.length} recipes)`;
        document.getElementById('importPreviewModal').classList.remove('hidden');
    }

    showToast(`Parsed ${pendingRecipeImportItems.length} recipes from file`, "info");
}

async function confirmRecipeImport() {
    if (pendingRecipeImportItems.length === 0) return;
    try {
        const res = await fetch(`${API_BASE}/api/import-recipes`, {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify({ recipes: pendingRecipeImportItems })
        });
        if (res.ok) {
            const result = await res.json();
            if (result.error) {
                showToast(`Failed to import: ${result.error}`, "error");
            } else {
                showToast(`Import Success! Imported: ${result.imported}, Skipped: ${result.skipped}`, "success");
                clearRecipeImportPreview();
                await initApp();
            }
        } else {
            showToast("Failed to process import on server", "error");
        }
    } catch (e) { 
        console.error(e);
        showToast("Connection error", "error"); 
    }
}

function clearRecipeImportPreview() {
    pendingRecipeImportItems = [];
    document.getElementById('recipeImportFile').value = '';
    document.getElementById('recipeImportPreviewArea').classList.add('hidden');
    document.getElementById('recipeImportPreviewList').innerHTML = '';
}

function downloadRecipeCsvTemplate() {
    const csvContent = "Recipe Name,Category,Price,Description,Item No,Ingredient,Ingredient,Measurement Unit\nFish Sandwich,Sandwiches,4.5,Fish Sandwich,R-101,Fish,1,Piece\nFish Sandwich,Sandwiches,4.5,Fish Sandwich,R-101,Burger Bun,1,Piece\nFish Sandwich,Sandwiches,4.5,Fish Sandwich,R-101,Lettuce,20,g\nFish Sandwich,Sandwiches,4.5,Fish Sandwich,R-101,Tartar Sauce,40,g\nFish Sandwich,Sandwiches,4.5,Fish Sandwich,R-101,Potato Fries,150,g\nChicken Burger,Burgers,5.5,Chicken Burger,R-102,Burger Bun,1,Piece\nChicken Burger,Burgers,5.5,Chicken Burger,R-102,Chicken Breast,180,g\nChicken Burger,Burgers,5.5,Chicken Burger,R-102,Lettuce,20,g\nChicken Burger,Burgers,5.5,Chicken Burger,R-102,Tomato,30,g\nChicken Burger,Burgers,5.5,Chicken Burger,R-102,Mayonnaise,25,g\nBeef Burger,Burgers,6.2,Beef Burger,R-103,Burger Bun,1,Piece\nBeef Burger,Burgers,6.2,Beef Burger,R-103,Beef Patty,200,g\nBeef Burger,Burgers,6.2,Beef Burger,R-103,Cheddar,25,g\nBeef Burger,Burgers,6.2,Beef Burger,R-103,Onion,20,g\nBeef Burger,Burgers,6.2,Beef Burger,R-103,Ketchup,20,g\n";
    
    fetch(`${API_BASE}/api/export-csv`, {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({ filename: "Recipes_Template.csv", csvContent })
    })
    .then(res => {
        if (res.ok) showToast("Recipe template downloaded to Downloads folder", "success");
        else showToast("Failed to download template", "error");
    })
    .catch(() => showToast("Connection error", "error"));
}

// ─── DAILY SALES IMPORT / EXPORT ─────────────────────────────────────
let pendingSalesItems = [];
let lastSalesDeductionResults = [];

function handleSalesImportFileSelect(e) {
    const file = e.target.files[0];
    if (!file) return;

    const reader = new FileReader();
    const isExcel = file.name.endsWith('.xlsx') || file.name.endsWith('.xls');

    if (isExcel) {
        reader.onload = function(evt) {
            try {
                const data = new Uint8Array(evt.target.result);
                const workbook = XLSX.read(data, { type: 'array' });
                const firstSheetName = workbook.SheetNames[0];
                const worksheet = workbook.Sheets[firstSheetName];
                const rows = XLSX.utils.sheet_to_json(worksheet, { header: 1, defval: "" });
                parseSalesImportRows(rows);
            } catch (err) {
                console.error(err);
                showToast("Failed to parse Excel file: " + err.message, "error");
            }
        };
        reader.readAsArrayBuffer(file);
    } else {
        reader.onload = function(evt) {
            const content = evt.target.result;
            let separator = ',';
            if (file.name.endsWith('.tsv')) {
                separator = '\t';
            } else {
                const headerLine = content.split('\n')[0] || '';
                const commaCount = (headerLine.match(/,/g) || []).length;
                const semiCount = (headerLine.match(/;/g) || []).length;
                const tabCount = (headerLine.match(/\t/g) || []).length;
                if (semiCount > commaCount && semiCount > tabCount) {
                    separator = ';';
                } else if (tabCount > commaCount && tabCount > semiCount) {
                    separator = '\t';
                } else {
                    separator = ',';
                }
            }
            const lines = content.split('\n').map(l => l.trim()).filter(l => l.length > 0);
            const rows = lines.map(line => parseCsvLine(line, separator));
            parseSalesImportRows(rows);
        };
        reader.readAsText(file);
    }
}

function parseSalesImportRows(rows) {
    if (rows.length <= 1) {
        showToast("Import file is empty", "warn");
        return;
    }

    const headers = rows[0].map(h => (h || '').toString().trim());
    
    const findColIdx = (synonyms) => {
        for (const syn of synonyms) {
            const cleanSyn = syn.toLowerCase().replace(/[\s_-]/g, '');
            const idx = headers.findIndex(h => {
                const cleanHeader = h.toLowerCase().replace(/[\s_-]/g, '');
                return cleanHeader === cleanSyn;
            });
            if (idx !== -1) return idx;
        }
        return -1;
    };

    const itemNoIdx = findColIdx(['item no', 'itemno', 'no.']);
    const recipeIdx = findColIdx(['recipe', 'recipe name', 'recipename', 'item', 'item name', 'itemname', 'meal', 'meal name', 'name', 'ingredient', 'ingredient name', 'description']);
    const qtyIdx = findColIdx(['qty sold', 'qtysold', 'qty', 'quantity', 'sold', 'quantity sold', 'count', 'quantity_sold', 'qsold', 'q sold']);
    const unitIdx = findColIdx(['unit', 'uom', 'unit of measure', 'unitofmeasure', 'unit of sale', 'unitofsale']);

    if (recipeIdx === -1 && itemNoIdx === -1) {
        showToast("Invalid file format. 'Item Name' or 'Item No' column is required.", "error");
        return;
    }
    const finalQtyIdx = qtyIdx !== -1 ? qtyIdx : -1;

    pendingSalesItems = [];
    for (let i = 1; i < rows.length; i++) {
        const cols = rows[i].map(c => (c === undefined || c === null ? "" : c).toString().trim());
        if (cols.length === 0 || (cols.length === 1 && !cols[0])) continue;

        const recipeName = recipeIdx !== -1 ? cols[recipeIdx] || '' : '';
        const qtySold = finalQtyIdx !== -1 ? parseInt(cols[finalQtyIdx]) || 1 : 1;
        const itemNo = itemNoIdx !== -1 ? cols[itemNoIdx] || '' : '';
        const unit = unitIdx !== -1 ? cols[unitIdx] || 'pcs' : 'pcs';

        if (recipeName || itemNo) {
            pendingSalesItems.push({
                itemNo,
                recipeName,
                qtySold,
                unitOfMeasure: unit
            });
        }
    }

    pendingImportItems = pendingSalesItems; // Set shared items for modal confirmation
    pendingImportType = 'sales'; // Set shared type for modal confirmation

    // Hide results area when a new file is uploaded
    const resultsArea = document.getElementById('salesImportResultsArea');
    if (resultsArea) resultsArea.classList.add('hidden');

    // Show preview in Import tab (if visible)
    if (document.getElementById('salesImportPreviewCount')) {
        document.getElementById('salesImportPreviewCount').innerText = pendingSalesItems.length;
    }
    const previewList = document.getElementById('salesImportPreviewList');
    if (previewList) {
        previewList.innerHTML = pendingSalesItems.map(item => `
            <div style="border-bottom:1px solid rgba(255,255,255,0.05); padding:8px 0; display:flex; justify-content:space-between; align-items:center;">
                <b style="color:var(--text-main); overflow:hidden; text-overflow:ellipsis; white-space:nowrap; max-width:70%;">${item.itemNo ? '[' + item.itemNo + '] ' : ''}${item.recipeName || 'Unnamed Item'}</b>
                <span style="color:var(--text-main); font-weight:700;">Qty: ${item.qtySold} ${item.unitOfMeasure || ''}</span>
            </div>
        `).join('');
    }
    const salesImportPreviewArea = document.getElementById('salesImportPreviewArea');
    if (salesImportPreviewArea) salesImportPreviewArea.classList.remove('hidden');

    // Show shared modal preview for immediate confirmation from the Sales Entry tab
    const modalPreviewList = document.getElementById('modalImportPreviewList');
    if (modalPreviewList) {
        modalPreviewList.innerHTML = pendingSalesItems.map(item => `
            <div style="border-bottom:1px solid rgba(255,255,255,0.05); padding:8px 0; display:flex; justify-content:space-between; align-items:center; text-align:left;">
                <b style="color:white; overflow:hidden; text-overflow:ellipsis; white-space:nowrap; max-width:70%;">${item.itemNo ? '[' + item.itemNo + '] ' : ''}${item.recipeName || 'Unnamed Item'}</b>
                <span style="color:var(--accent); font-weight:700;">Qty: ${item.qtySold} ${item.unitOfMeasure || ''}</span>
            </div>
        `).join('');
        document.getElementById('modalImportTitle').innerText = `Confirm Bulk Import (${pendingSalesItems.length} sales)`;
        document.getElementById('importPreviewModal').classList.remove('hidden');
    }

    showToast(`Parsed ${pendingSalesItems.length} recipe sales from file`, "info");
}

function clearSalesImportPreview() {
    pendingSalesItems = [];
    document.getElementById('salesImportFile').value = '';
    document.getElementById('salesImportPreviewArea').classList.add('hidden');
    document.getElementById('salesImportPreviewList').innerHTML = '';
}

async function confirmSalesImport() {
    if (pendingSalesItems.length === 0) return;
    const loader = document.getElementById('loadingOverlay');
    if (loader) loader.classList.remove('hidden');
    try {
        const res = await fetch(`${API_BASE}/api/import-sales`, {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify({ sales: pendingSalesItems })
        });
        if (res.ok) {
            const result = await res.json();
            showToast(`Deduction complete! Processed: ${result.processed}, Skipped: ${result.skipped}`, "success");
            
            // Store deductions for CSV export
            lastSalesDeductionResults = result.deductions || [];

            // Display Results (checking if elements exist)
            const processedEl = document.getElementById('salesResultProcessed');
            if (processedEl) processedEl.innerText = result.processed;
            
            const skippedEl = document.getElementById('salesResultSkipped');
            if (skippedEl) skippedEl.innerText = result.skipped;

            const skippedContainer = document.getElementById('salesResultSkippedListContainer');
            if (skippedContainer) {
                if (result.skipped > 0 && result.skippedNames && result.skippedNames.length > 0) {
                    document.getElementById('salesResultSkippedList').innerText = result.skippedNames.join(', ');
                    skippedContainer.classList.remove('hidden');
                } else {
                    skippedContainer.classList.add('hidden');
                }
            }

            const resultsList = document.getElementById('salesImportResultsList');
            if (resultsList) {
                if (lastSalesDeductionResults.length > 0) {
                    resultsList.innerHTML = lastSalesDeductionResults.map(d => {
                        const partId = d.partId !== undefined ? d.partId : d.PartId;
                        const partName = d.partName || d.PartName || "";
                        const qtyDeducted = d.qtyDeducted !== undefined ? d.qtyDeducted : d.QtyDeducted;
                        const newStock = d.newStock !== undefined ? d.newStock : d.NewStock;

                        const p = allProducts.find(x => x.id === partId);

                        let qtyStr = "";
                        let stockStr = "";
                        if (p && p.bigUnit) {
                            qtyStr = `-${qtyDeducted.toFixed(2).replace(/\.00$/, '')} ${p.bigUnit}`;
                            stockStr = `Stock: ${newStock.toFixed(2).replace(/\.00$/, '')} ${p.bigUnit}`;
                        } else {
                            qtyStr = `-${qtyDeducted.toFixed(2).replace(/\.00$/, '')}`;
                            stockStr = `Stock: ${newStock.toFixed(2).replace(/\.00$/, '')}`;
                        }

                        return `
                            <div style="border-bottom:1px solid rgba(255,255,255,0.05); padding:6px 0; display:grid; grid-template-columns:1.5fr 1fr 1fr; gap:10px;">
                                <b style="color:var(--text-main); overflow:hidden; text-overflow:ellipsis; white-space:nowrap;">${partName}</b>
                                <span style="color:var(--danger); text-align:right;">${qtyStr}</span>
                                <span style="color:${newStock <= 0 ? 'var(--danger)' : newStock < 5 ? 'var(--warn)' : 'var(--text-main)'}; text-align:right; font-weight:700;">${stockStr}</span>
                            </div>
                        `;
                    }).join('');
                } else {
                    resultsList.innerHTML = '<div style="color:var(--text-muted); text-align:center; padding:10px;">No ingredient deductions occurred.</div>';
                }
            }

            const resultsArea = document.getElementById('salesImportResultsArea');
            if (resultsArea) resultsArea.classList.remove('hidden');
            
            clearSalesImportPreview();
            clearImportPreview();

            // Refresh the application inventory cache & reload tables immediately!
            await initApp();
            await loadSalesTab();
            loadInventoryTable();
        } else {
            const errText = await res.text();
            showToast("Server error: " + errText, "error");
        }
    } catch (e) {
        console.error(e);
        showToast("Error: " + e.message, "error");
    } finally {
        if (loader) loader.classList.add('hidden');
    }
}

function downloadSalesCsvTemplate() {
    const csvContent = "Item No,Item Name,Qty Sold,Unit\n101,Fish Sandwich,24,pcs\n102,Chicken Burger,57,pcs\n103,Beef Burger,53,pcs\n1001,BBQ Sauce,5,L\n";
    
    fetch(`${API_BASE}/api/export-csv`, {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({ filename: "Sales_Import_Template.csv", csvContent })
    })
    .then(res => {
        if (res.ok) showToast("Template downloaded to Downloads folder", "success");
        else showToast("Failed to download template", "error");
    })
    .catch(() => showToast("Connection error", "error"));
}

async function downloadSalesConsumptionReport() {
    if (lastSalesDeductionResults.length === 0) {
        showToast("No consumption results to export", "warn");
        return;
    }
    try {
        const headers = ["Ingredient Name", "Quantity Deducted", "Previous Stock", "New Stock", "Unit"];
        const rows = lastSalesDeductionResults.map(d => {
            const partId = d.partId !== undefined ? d.partId : d.PartId;
            const partName = d.partName || d.PartName || "";
            const qtyDeducted = d.qtyDeducted !== undefined ? d.qtyDeducted : d.QtyDeducted;
            const previousStock = d.previousStock !== undefined ? d.previousStock : d.PreviousStock;
            const newStock = d.newStock !== undefined ? d.newStock : d.NewStock;

            const p = allProducts.find(x => x.id === partId);

            let qVal = qtyDeducted;
            let pVal = previousStock;
            let nVal = newStock;
            let unit = "pcs";

            if (p && p.bigUnit) {
                unit = p.bigUnit;
            }

            return [
                `"${partName.replace(/"/g, '""')}"`,
                qVal.toFixed(4).replace(/\.?0+$/, ''),
                pVal.toFixed(4).replace(/\.?0+$/, ''),
                nVal.toFixed(4).replace(/\.?0+$/, ''),
                `"${unit}"`
            ];
        });

        const csvContent = [headers.join(','), ...rows.map(r => r.join(','))].join('\n');
        const filename = `sales_consumption_${new Date().toISOString().slice(0,10)}.csv`;

        const res = await fetch(`${API_BASE}/api/export-csv`, {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify({ filename, csvContent })
        });

        if (res.ok) {
            const data = await res.json();
            showToast(`Consumption report saved successfully to ${data.path}!`, "success");
        } else {
            showToast("Failed to export report on device", "error");
        }
    } catch (e) { showToast("Connection error during consumption export", "error"); }
}

// ─── REPORTS & ANALYTICS ──────────────────────────────────────────────
let reportCharts = {};
window.currentReportData = null;

window.toggleReportPeriodType = function() {
    const filterTypeSelect = document.getElementById('reportFilterType');
    const filterType = filterTypeSelect ? filterTypeSelect.value : 'monthly';
    const monthlyDiv = document.getElementById('monthlyReportFilters');
    const dailyDiv = document.getElementById('dailyReportFilters');
    const dateInput = document.getElementById('reportFilterDate');
    
    if (filterType === 'daily') {
        if (monthlyDiv) monthlyDiv.style.display = 'none';
        if (dailyDiv) dailyDiv.style.display = 'flex';
        if (dateInput && !dateInput.value) {
            const today = new Date();
            const yyyy = today.getFullYear();
            const mm = String(today.getMonth() + 1).padStart(2, '0');
            const dd = String(today.getDate()).padStart(2, '0');
            dateInput.value = `${yyyy}-${mm}-${dd}`;
        }
    } else {
        if (monthlyDiv) monthlyDiv.style.display = 'flex';
        if (dailyDiv) dailyDiv.style.display = 'none';
    }
    loadReportsData();
};

async function loadReportsData() {
    try {
        const monthSelect = document.getElementById('reportFilterMonth');
        const yearSelect = document.getElementById('reportFilterYear');
        const dateInput = document.getElementById('reportFilterDate');
        const filterTypeSelect = document.getElementById('reportFilterType');
        const filterType = filterTypeSelect ? filterTypeSelect.value : 'monthly';
        
        // Auto-initialize month/year selects to current date if not set
        if (monthSelect && !monthSelect.dataset.initialized) {
            monthSelect.value = new Date().getMonth() + 1;
            monthSelect.dataset.initialized = 'true';
        }
        if (yearSelect && !yearSelect.dataset.initialized) {
            yearSelect.value = new Date().getFullYear();
            yearSelect.dataset.initialized = 'true';
        }
        if (dateInput && !dateInput.value) {
            const today = new Date();
            const yyyy = today.getFullYear();
            const mm = String(today.getMonth() + 1).padStart(2, '0');
            const dd = String(today.getDate()).padStart(2, '0');
            dateInput.value = `${yyyy}-${mm}-${dd}`;
        }

        let url = `${API_BASE}/api/reports`;
        if (filterType === 'daily' && dateInput && dateInput.value) {
            const parts = dateInput.value.split('-');
            if (parts.length === 3) {
                url += `?year=${parts[0]}&month=${parseInt(parts[1], 10)}&day=${parseInt(parts[2], 10)}`;
            } else {
                url += `?month=${new Date().getMonth() + 1}&year=${new Date().getFullYear()}`;
            }
        } else {
            const month = monthSelect ? monthSelect.value : (new Date().getMonth() + 1);
            const year = yearSelect ? yearSelect.value : new Date().getFullYear();
            url += `?month=${month}&year=${year}`;
        }

        const res = await fetch(url);
        if (res.ok) {
            const data = await res.json();
            window.currentReportData = data;
            
            // 1. Populate KPI Cards
            document.getElementById('kpiRevenue').innerText = formatPrice(data.revenue || 0);
            document.getElementById('kpiOrders').innerText = data.orders || 0;
            document.getElementById('kpiCOGS').innerText = formatPrice(data.cogs || 0);
            document.getElementById('kpiProfit').innerText = formatPrice(data.grossProfit || 0);
            document.getElementById('kpiMargin').innerText = (data.profitMargin || 0).toFixed(2) + '%';
            document.getElementById('kpiInvValue').innerText = formatPrice(data.financialSummary?.inventoryValue || 0);
            document.getElementById('kpiLowStock').innerText = data.lowStock || 0;
            document.getElementById('kpiOutOfStock').innerText = data.outOfStock || 0;

            const showCost = shouldShowCost();
            
            const btnSubtabRecipes = document.getElementById('btnSubtabRecipes');
            if (btnSubtabRecipes) btnSubtabRecipes.style.display = showCost ? 'inline-block' : 'none';

            const cogsCard = document.getElementById('kpiCOGS')?.closest('.glass-card');
            if (cogsCard) cogsCard.style.display = showCost ? 'block' : 'none';

            const profitCard = document.getElementById('kpiProfit')?.closest('.glass-card');
            if (profitCard) profitCard.style.display = showCost ? 'block' : 'none';

            const marginCard = document.getElementById('kpiMargin')?.closest('.glass-card');
            if (marginCard) marginCard.style.display = showCost ? 'block' : 'none';

            const invValCard = document.getElementById('kpiInvValue')?.closest('.glass-card');
            if (invValCard) invValCard.style.display = showCost ? 'block' : 'none';

            const finPurchaseRow = document.getElementById('finPurchaseCost')?.closest('tr');
            if (finPurchaseRow) finPurchaseRow.style.display = showCost ? 'table-row' : 'none';

            const finIngredientRow = document.getElementById('finIngredientCost')?.closest('tr');
            if (finIngredientRow) finIngredientRow.style.display = showCost ? 'table-row' : 'none';

            const finGrossProfitRow = document.getElementById('finGrossProfit')?.closest('tr');
            if (finGrossProfitRow) finGrossProfitRow.style.display = showCost ? 'table-row' : 'none';

            const finProfitMarginRow = document.getElementById('finProfitMargin')?.closest('tr');
            if (finProfitMarginRow) finProfitMarginRow.style.display = showCost ? 'table-row' : 'none';

            // 2. Populate Financial Statements Summary
            document.getElementById('finRevenue').innerText = formatPrice(data.revenue || 0);
            document.getElementById('finPurchaseCost').innerText = formatPrice(data.financialSummary?.purchaseCost || 0);
            document.getElementById('finIngredientCost').innerText = formatPrice(data.cogs || 0);
            document.getElementById('finGrossProfit').innerText = formatPrice(data.grossProfit || 0);
            document.getElementById('finProfitMargin').innerText = (data.profitMargin || 0).toFixed(2) + '%';

            // 3. Render Category Sales List
            const catList = document.getElementById('categorySalesList');
            if (catList) {
                catList.innerHTML = '';
                if (data.categorySales && data.categorySales.length > 0) {
                    const maxSales = Math.max(...data.categorySales.map(c => c.sales)) || 1;
                    data.categorySales.forEach(c => {
                        const pct = (c.sales / maxSales) * 100;
                        const row = document.createElement('div');
                        row.className = 'report-bar-row';
                        row.innerHTML = `
                            <div class="report-bar-label">
                                <span style="font-weight:600; color:var(--text-main);">${c.category}</span>
                                <span style="color:var(--accent); font-weight:700;">${formatPrice(c.sales)}</span>
                            </div>
                            <div class="report-bar-outer">
                                <div class="report-bar-inner" style="width: ${pct}%;"></div>
                            </div>
                        `;
                        catList.appendChild(row);
                    });
                } else {
                    catList.innerHTML = '<div style="color:var(--text-muted); font-size:0.9rem; text-align:center; padding:20px;">No sales data available.</div>';
                }
            }

            // 4. Render Recipe Sales Summary Table
            const recipeTbody = document.getElementById('reportsRecipeTableBody');
            if (recipeTbody) {
                recipeTbody.innerHTML = '';
                if (data.recipeSummary && data.recipeSummary.length > 0) {
                    data.recipeSummary.forEach(r => {
                        const tr = document.createElement('tr');
                        tr.innerHTML = `
                            <td class="col-rep-name" style="padding:12px; font-weight:600; text-align:left;">${r.name}</td>
                            <td data-label="Quantity Sold" style="padding:12px; text-align:center;">${r.qtySold}</td>
                            <td data-label="Revenue" style="padding:12px; text-align:right; font-weight:bold; color:var(--accent);">${formatPrice(r.revenue)}</td>
                            <td data-label="Cost" style="padding:12px; text-align:right; color:#f87171;">${formatPrice(r.cost)}</td>
                            <td data-label="Profit" style="padding:12px; text-align:right; font-weight:bold; color:#34d399;">${formatPrice(r.profit)}</td>
                        `;
                        recipeTbody.appendChild(tr);
                    });
                } else {
                    recipeTbody.innerHTML = '<tr><td colspan="5" style="text-align:center; padding:20px; color:var(--text-muted);">No recipe sales recorded in this period.</td></tr>';
                }
            }

            // 5. Render Ingredient Consumption Table
            const ingTbody = document.getElementById('reportsIngredientTableBody');
            if (ingTbody) {
                ingTbody.innerHTML = '';
                if (data.ingredientConsumption && data.ingredientConsumption.length > 0) {
                    data.ingredientConsumption.forEach(i => {
                        const tr = document.createElement('tr');
                        tr.innerHTML = `
                            <td class="col-rep-name" style="padding:12px; font-weight:600; text-align:left;">${i.name}</td>
                            <td data-label="Opening Stock" style="padding:12px; text-align:center;">${i.openingStock}</td>
                            <td data-label="Purchased / Added" style="padding:12px; text-align:center; color:#34d399;">+${i.purchased}</td>
                            <td data-label="Used in Recipes" style="padding:12px; text-align:center; color:#f87171;">-${i.usedInRecipes}</td>
                            <td data-label="Sold Directly" style="padding:12px; text-align:center; color:#fbbf24;">-${i.soldDirectly}</td>
                            <td data-label="Closing Stock" style="padding:12px; text-align:center; font-weight:bold;">${i.closingStock}</td>
                            <td data-label="Unit" style="padding:12px; text-align:center; color:var(--text-muted); font-size:0.85rem;">${i.unit}</td>
                        `;
                        ingTbody.appendChild(tr);
                    });
                } else {
                    ingTbody.innerHTML = '<tr><td colspan="7" style="text-align:center; padding:20px; color:var(--text-muted);">No ingredient logs found.</td></tr>';
                }
            }

            // 6. Render Stock Movement History Table
            const movTbody = document.getElementById('reportsMovementTableBody');
            if (movTbody) {
                movTbody.innerHTML = '';
                if (data.stockMovement && data.stockMovement.length > 0) {
                    data.stockMovement.forEach(m => {
                        let typeColor = 'inherit';
                        let prefix = '';
                        if (m.type === 'Purchase') { typeColor = '#34d399'; prefix = '+'; }
                        else if (m.type === 'Ingredient Sale' || m.type === 'Recipe Consumption') { typeColor = '#f87171'; prefix = '-'; }
                        else if (m.type.includes('Adjustment')) { typeColor = '#fbbf24'; prefix = m.quantity < 0 ? '-' : '+'; }

                        const tr = document.createElement('tr');
                        tr.innerHTML = `
                            <td class="col-rep-date" style="padding:12px; text-align:left; font-size:0.85rem; color:var(--text-muted);">${m.date}</td>
                            <td data-label="Ingredient" style="padding:12px; font-weight:600; text-align:left;">${m.ingredient}</td>
                            <td data-label="Movement Type" style="padding:12px; text-align:center; font-weight:bold; color:${typeColor};">${m.type}</td>
                            <td data-label="Quantity Change" style="padding:12px; text-align:center; font-weight:bold; color:${typeColor};">${prefix}${Math.abs(m.quantity)}</td>
                            <td data-label="Unit" style="padding:12px; text-align:center; color:var(--text-muted); font-size:0.85rem;">${m.unit}</td>
                            <td data-label="Remaining Stock" style="padding:12px; text-align:center; font-weight:bold;">${m.remainingStock}</td>
                        `;
                        movTbody.appendChild(tr);
                    });
                } else {
                    movTbody.innerHTML = '<tr><td colspan="6" style="text-align:center; padding:20px; color:var(--text-muted);">No stock movements recorded in this period.</td></tr>';
                }
            }

            // 7. Render Charts
            if (data.chartsData) {
                renderReportCharts(data.chartsData);
            }
        }
    } catch (e) { console.error("Failed to load reports", e); }
}

function renderReportCharts(chartsData) {
    try {
        const isDarkMode = document.body.classList.contains('dark-mode');
        const themeColorText = isDarkMode ? 'rgba(255, 255, 255, 0.7)' : 'rgba(30, 41, 59, 0.85)';
        const themeColorGrid = isDarkMode ? 'rgba(255, 255, 255, 0.1)' : 'rgba(0, 0, 0, 0.08)';

        // Destroy existing charts to prevent overlaps
        if (reportCharts.salesDay) reportCharts.salesDay.destroy();
        if (reportCharts.revTrend) reportCharts.revTrend.destroy();
        if (reportCharts.bestRecipes) reportCharts.bestRecipes.destroy();
        if (reportCharts.mostIngredients) reportCharts.mostIngredients.destroy();

        const commonOptions = {
            responsive: true,
            maintainAspectRatio: false,
            plugins: {
                legend: { labels: { color: themeColorText, font: { family: 'Inter', weight: 'bold' } } }
            },
            scales: {
                x: { grid: { color: themeColorGrid }, ticks: { color: themeColorText } },
                y: { grid: { color: themeColorGrid }, ticks: { color: themeColorText } }
            }
        };

        // 1. Sales by Day
        const ctxSales = document.getElementById('chartSalesDay')?.getContext('2d');
        if (ctxSales) {
            const labels = Object.keys(chartsData.salesByDay || {});
            const data = Object.values(chartsData.salesByDay || {});
            reportCharts.salesDay = new Chart(ctxSales, {
                type: 'line',
                data: {
                    labels: labels,
                    datasets: [{
                        label: 'Daily Sales ($)',
                        data: data,
                        borderColor: '#06b6d4',
                        backgroundColor: 'rgba(6, 182, 212, 0.1)',
                        fill: true,
                        tension: 0.3,
                        borderWidth: 3
                    }]
                },
                options: commonOptions
            });
        }

        // 2. Monthly Revenue Trend
        const ctxTrend = document.getElementById('chartRevTrend')?.getContext('2d');
        if (ctxTrend) {
            const labels = (chartsData.revenueTrend || []).map(x => x.month);
            const data = (chartsData.revenueTrend || []).map(x => x.revenue);
            reportCharts.revTrend = new Chart(ctxTrend, {
                type: 'line',
                data: {
                    labels: labels,
                    datasets: [{
                        label: 'Monthly Revenue Trend ($)',
                        data: data,
                        borderColor: '#10b981',
                        backgroundColor: 'rgba(16, 185, 129, 0.1)',
                        fill: true,
                        tension: 0.2,
                        borderWidth: 3
                    }]
                },
                options: commonOptions
            });
        }

        // 3. Best Selling Recipes
        const ctxRecipes = document.getElementById('chartBestRecipes')?.getContext('2d');
        if (ctxRecipes) {
            const labels = (chartsData.bestRecipes || []).map(x => x.name);
            const data = (chartsData.bestRecipes || []).map(x => x.qty);
            reportCharts.bestRecipes = new Chart(ctxRecipes, {
                type: 'bar',
                data: {
                    labels: labels,
                    datasets: [{
                        label: 'Portions Sold',
                        data: data,
                        backgroundColor: '#3b82f6',
                        borderRadius: 6
                    }]
                },
                options: commonOptions
            });
        }

        // 4. Most Consumed Ingredients
        const ctxIngredients = document.getElementById('chartMostIngredients')?.getContext('2d');
        if (ctxIngredients) {
            const labels = (chartsData.mostIngredients || []).map(x => x.name);
            const data = (chartsData.mostIngredients || []).map(x => x.qty);
            reportCharts.mostIngredients = new Chart(ctxIngredients, {
                type: 'doughnut',
                data: {
                    labels: labels,
                    datasets: [{
                        data: data,
                        backgroundColor: ['#06b6d4', '#10b981', '#3b82f6', '#fbbf24', '#f87171'],
                        borderWidth: 0
                    }]
                },
                options: {
                    responsive: true,
                    maintainAspectRatio: false,
                    plugins: {
                        legend: { position: 'right', labels: { color: themeColorText } }
                    }
                }
            });
        }
    } catch (err) { console.error("Failed to render Chart.js graphs", err); }
}

window.switchReportSubtab = function(subtabId) {
    const panels = document.querySelectorAll('.reports-panel');
    panels.forEach(p => p.classList.add('hidden'));

    const selectedPanel = document.getElementById(`panelReports${subtabId.charAt(0).toUpperCase() + subtabId.slice(1)}`);
    if (selectedPanel) selectedPanel.classList.remove('hidden');

    const tabButtons = document.querySelectorAll('.btn-subtab');
    tabButtons.forEach(btn => btn.classList.remove('active'));

    const activeBtn = document.getElementById(`btnSubtab${subtabId.charAt(0).toUpperCase() + subtabId.slice(1)}`);
    if (activeBtn) activeBtn.classList.add('active');
};

window.exportReport = async function(format) {
    const data = window.currentReportData;
    if (!data) {
        showToast("No report data available to export.", "error");
        return;
    }

    const filterTypeSelect = document.getElementById('reportFilterType');
    const filterType = filterTypeSelect ? filterTypeSelect.value : 'monthly';
    const isDaily = filterType === 'daily';

    let reportTitle = "MONTHLY BUSINESS ACTIVITY REPORT";
    let periodText = "";
    let filePeriodSuffix = "";

    if (isDaily) {
        reportTitle = "DAILY BUSINESS ACTIVITY REPORT";
        const dateInput = document.getElementById('reportFilterDate');
        periodText = dateInput ? dateInput.value : "";
        filePeriodSuffix = periodText.replace(/-/g, '_');
    } else {
        const monthSelect = document.getElementById('reportFilterMonth');
        const yearSelect = document.getElementById('reportFilterYear');
        const monthName = monthSelect.options[monthSelect.selectedIndex].text;
        const yearVal = yearSelect.value;
        periodText = `${monthName} ${yearVal}`;
        filePeriodSuffix = `${monthName.replace(/\s+/g, '_')}_${yearVal}`;
    }

    const prefix = isDaily ? 'daily_report' : 'monthly_report';

    if (format === 'pdf') {
        const periodEl = document.getElementById('printReportPeriod');
        if (periodEl) periodEl.innerText = `Selected Period: ${periodText}`;
        const titleEl = document.getElementById('printReportTitle');
        if (titleEl) titleEl.innerText = isDaily ? 'Daily Business Activity Report' : 'Monthly Business Activity Report';
        if (window.AndroidBridge && typeof window.AndroidBridge.printPage === 'function') {
            window.AndroidBridge.printPage(`${prefix}_${filePeriodSuffix}`);
        } else {
            window.print();
        }
        return;
    }

    try {
        if (format === 'excel') {
            const workbook = XLSX.utils.book_new();

            // Sheet 1: Summary
            const summaryData = [
                [`${reportTitle} - ${periodText.toUpperCase()}`],
                [],
                ["FINANCIAL SUMMARY"],
                ["Total Sales Revenue", data.revenue],
                ["Total Sales Count", data.orders],
                ["Cost of Goods Sold (COGS)", data.cogs],
                ["Gross Profit", data.grossProfit],
                ["Profit Margin", `${(data.profitMargin || 0).toFixed(2)}%`],
                ["Inventory Asset Value", data.financialSummary?.inventoryValue || 0]
            ];
            const wsSummary = XLSX.utils.aoa_to_sheet(summaryData);
            XLSX.utils.book_append_sheet(workbook, wsSummary, "Summary");

            // Sheet 2: Recipe Sales Summary
            const recipeHeaders = ["Recipe Name", "Quantity Sold", "Revenue", "Cost of Ingredients", "Net Profit"];
            const recipeRows = (data.recipeSummary || []).map(r => [
                r.name,
                r.qtySold,
                r.revenue,
                r.cost,
                r.profit
            ]);
            const wsRecipes = XLSX.utils.aoa_to_sheet([recipeHeaders, ...recipeRows]);
            XLSX.utils.book_append_sheet(workbook, wsRecipes, "Recipe Sales");

            // Sheet 3: Ingredient Consumption
            const ingHeaders = ["Ingredient Name", "Opening Stock", "Purchased Qty", "Quantity Used in Recipes", "Quantity Sold Directly", "Closing Stock", "Unit"];
            const ingRows = (data.ingredientConsumption || []).map(i => [
                i.name,
                i.openingStock,
                i.purchased,
                i.usedInRecipes,
                i.soldDirectly,
                i.closingStock,
                i.unit
            ]);
            const wsIng = XLSX.utils.aoa_to_sheet([ingHeaders, ...ingRows]);
            XLSX.utils.book_append_sheet(workbook, wsIng, "Ingredient Consumption");

            // Sheet 4: Stock Movement History
            const moveHeaders = ["Date & Time", "Ingredient", "Movement Type", "Quantity Change", "Unit", "Remaining Stock"];
            const moveRows = (data.stockMovement || []).map(m => [
                m.date,
                m.ingredient,
                m.type,
                m.quantity,
                m.unit,
                m.remainingStock
            ]);
            const wsMove = XLSX.utils.aoa_to_sheet([moveHeaders, ...moveRows]);
            XLSX.utils.book_append_sheet(workbook, wsMove, "Stock Movement");

            const base64Content = XLSX.write(workbook, { bookType: 'xlsx', type: 'base64' });
            const filename = `${prefix}_${filePeriodSuffix}.xlsx`;

            await sendFileToBackend(filename, null, base64Content);
        } else {
            const filename = `${prefix}_${filePeriodSuffix}.csv`;
            let csv = "";
            
            // 1. Title
            csv += `${reportTitle} - ${periodText.toUpperCase()}\n\n`;

            // 2. Financial Summary Section
            csv += "FINANCIAL SUMMARY\n";
            csv += `Total Sales Revenue,${data.revenue.toFixed(2)}\n`;
            csv += `Total Sales Count,${data.orders}\n`;
            csv += `Cost of Goods Sold (COGS),${data.cogs.toFixed(2)}\n`;
            csv += `Gross Profit,${data.grossProfit.toFixed(2)}\n`;
            csv += `Profit Margin,${(data.profitMargin || 0).toFixed(2)}%\n`;
            csv += `Inventory Asset Value,${(data.financialSummary?.inventoryValue || 0).toFixed(2)}\n\n`;

            // 3. Recipe Summary Table
            csv += "RECIPE SALES SUMMARY\n";
            csv += "Recipe Name,Quantity Sold,Revenue,Cost of Ingredients,Net Profit\n";
            if (data.recipeSummary && data.recipeSummary.length > 0) {
                data.recipeSummary.forEach(r => {
                    csv += `"${r.name}",${r.qtySold},${r.revenue.toFixed(2)},${r.cost.toFixed(2)},${r.profit.toFixed(2)}\n`;
                });
            } else {
                csv += "No records found\n";
            }
            csv += "\n";

            // 4. Ingredient Consumption Table
            csv += "INGREDIENT CONSUMPTION\n";
            csv += "Ingredient Name,Opening Stock,Purchased Qty,Quantity Used in Recipes,Quantity Sold Directly,Closing Stock,Unit\n";
            if (data.ingredientConsumption && data.ingredientConsumption.length > 0) {
                data.ingredientConsumption.forEach(i => {
                    csv += `"${i.name}",${i.openingStock},${i.purchased},${i.usedInRecipes},${i.soldDirectly},${i.closingStock},"${i.unit}"\n`;
                });
            } else {
                csv += "No records found\n";
            }
            csv += "\n";

            // 5. Stock Movement History Table
            csv += "STOCK MOVEMENT HISTORY\n";
            csv += "Date & Time,Ingredient,Movement Type,Quantity Change,Unit,Remaining Stock\n";
            if (data.stockMovement && data.stockMovement.length > 0) {
                data.stockMovement.forEach(m => {
                    csv += `"${m.date}","${m.ingredient}","${m.type}",${m.quantity},"${m.unit}",${m.remainingStock}\n`;
                });
            } else {
                csv += "No records found\n";
            }

            await sendFileToBackend(filename, csv, null);
        }
    } catch (e) {
        showToast("Error exporting report file.", "error");
        console.error(e);
    }
};

window.clearReportsData = function() {
    showDeleteConfirm(
        "Clear Daily Report",
        "Are you sure you want to clear all sales orders and activity logs to start a fresh daily report? This action cannot be undone.",
        async () => {
            try {
                const res = await fetch(`${API_BASE}/api/clear-reports`, { method: 'POST' });
                if (res.ok) {
                    showToast("Daily report cleared successfully!", "success");
                    await loadReportsData();
                } else {
                    showToast("Failed to clear reports", "error");
                }
            } catch (e) {
                showToast("Connection error", "error");
            }
        },
        "Clear"
    );
};


window.toggleCartDrawer = function() {
    const sidebar = document.querySelector('.cart-sidebar');
    const closeBtn = document.querySelector('.cart-close-btn');
    if (sidebar) {
        const isOpen = sidebar.classList.toggle('open');
        if (closeBtn) {
            closeBtn.style.display = isOpen ? 'block' : 'none';
        }
    }
};


// ─── MANUAL SALES ENTRY TAB ───────────────────────────────────────────
window.loadSalesTab = async function() {
    // 1. Populate product & recipe dropdown with optgroups
    const select = document.getElementById('salesProductSelect');
    if (select) {
        // Keep initial option
        select.innerHTML = '<option value="">Select an item...</option>';
        
        // Products group
        const prodGroup = document.createElement('optgroup');
        prodGroup.label = "Ingredients & Products";
        const sortedProducts = [...allProducts].sort((a, b) => a.name.localeCompare(b.name));
        sortedProducts.forEach(p => {
            if (p.status !== 'Inactive') {
                const opt = document.createElement('option');
                opt.value = `prod-${p.id}`;
                opt.textContent = `${p.name} ($${p.price.toFixed(2)})`;
                prodGroup.appendChild(opt);
            }
        });
        select.appendChild(prodGroup);

        // Recipes group
        const recGroup = document.createElement('optgroup');
        recGroup.label = "Food Recipes";
        const sortedRecipes = [...allRecipes].sort((a, b) => a.name.localeCompare(b.name));
        sortedRecipes.forEach(r => {
            if (r.status !== 'Inactive') {
                const opt = document.createElement('option');
                opt.value = `rec-${r.id}`;
                opt.textContent = `${r.name} ($${r.price.toFixed(2)})`;
                recGroup.appendChild(opt);
            }
        });
        select.appendChild(recGroup);
    }

    // Set default date filter to today if not set
    const dateFilter = document.getElementById('salesHistoryDateFilter');
    if (dateFilter && !dateFilter.value) {
        dateFilter.value = new Date().toISOString().slice(0, 10);
    }
    
    await window.loadSalesHistoryList();
    
    // Reset Form
    if (select) select.value = "";
    const qtyInput = document.getElementById('salesQuantityInput');
    if (qtyInput) qtyInput.value = "1";
    const priceInput = document.getElementById('salesPriceInput');
    if (priceInput) priceInput.value = "0.00";
    const uomGroup = document.getElementById('salesUomGroup');
    if (uomGroup) uomGroup.style.display = 'none';
    calculateSalesTotal();

    if (typeof window.refreshCustomSalesDropdown === 'function') {
        window.refreshCustomSalesDropdown();
    }
};

window.onSalesProductChange = function() {
    const select = document.getElementById('salesProductSelect');
    const priceInput = document.getElementById('salesPriceInput');
    const uomGroup = document.getElementById('salesUomGroup');
    const uomSelect = document.getElementById('salesUomSelect');
    if (!select || !priceInput) return;

    if (!select.value) {
        priceInput.value = "0.00";
        if (uomGroup) uomGroup.style.display = 'none';
        calculateSalesTotal();
        return;
    }

    const [type, idStr] = select.value.split('-');
    const itemId = parseInt(idStr);

    if (type === 'prod') {
        const product = allProducts.find(p => p.id === itemId);
        if (product) {
            const convVal = parseFloat(product.conversionValue) || 1.0;
            if (product.bigUnit && product.smallUnit && uomSelect && uomGroup) {
                uomSelect.innerHTML = `
                    <option value="${product.smallUnit.toLowerCase()}">${product.smallUnit}</option>
                    <option value="${product.bigUnit.toLowerCase()}">${product.bigUnit}</option>
                `;
                uomGroup.style.display = 'block';
                priceInput.value = formatDynamicNumber(product.price / convVal);
            } else {
                priceInput.value = product.price.toFixed(2);
                if (uomGroup) uomGroup.style.display = 'none';
            }
        }
    } else if (type === 'rec') {
        const recipe = allRecipes.find(r => r.id === itemId);
        if (recipe) {
            priceInput.value = recipe.price.toFixed(2);
        }
        if (uomGroup) uomGroup.style.display = 'none';
    }
    calculateSalesTotal();
};

window.onSalesUomChange = function() {
    const select = document.getElementById('salesProductSelect');
    const uomSelect = document.getElementById('salesUomSelect');
    const priceInput = document.getElementById('salesPriceInput');
    if (!select || !uomSelect || !priceInput) return;

    const [type, idStr] = select.value.split('-');
    const itemId = parseInt(idStr);
    if (type !== 'prod') return;

    const product = allProducts.find(p => p.id === itemId);
    if (!product) return;

    const val = uomSelect.value;
    const basePrice = product.price;
    const convVal = parseFloat(product.conversionValue) || 1.0;

    if (val === product.bigUnit.toLowerCase()) {
        priceInput.value = basePrice.toFixed(2);
    } else if (val === product.smallUnit.toLowerCase()) {
        priceInput.value = formatDynamicNumber(basePrice / convVal);
    }
    calculateSalesTotal();
};

window.calculateSalesTotal = function() {
    const qtyInput = document.getElementById('salesQuantityInput');
    const priceInput = document.getElementById('salesPriceInput');
    const totalDisplay = document.getElementById('salesTotalDisplay');
    if (!qtyInput || !priceInput || !totalDisplay) return;

    const qty = parseFloat(qtyInput.value) || 0;
    const price = parseFloat(priceInput.value) || 0;
    const total = qty * price;
    totalDisplay.textContent = `$${total.toFixed(2)}`;
};

function renderSalesItemsTable(items) {
    const tbody = document.getElementById('salesTableBody');
    if (!tbody) return;
    tbody.innerHTML = '';

    if (!items || items.length === 0) {
        tbody.innerHTML = '<tr><td colspan="6" style="text-align:center; padding:20px; color:var(--text-muted);">No sales entries found.</td></tr>';
        return;
    }

    items.forEach(item => {
        const tr = document.createElement('tr');
        const formattedDate = new Date(item.date).toLocaleString();
        
        tr.innerHTML = `
            <td style="padding:12px; border-bottom:1px solid var(--border); color:var(--text-main); font-weight:600;">${item.name}</td>
            <td style="padding:12px; border-bottom:1px solid var(--border); text-align:right;">${item.qty} ${item.unitOfMeasure || ''}</td>
            <td style="padding:12px; border-bottom:1px solid var(--border); text-align:right;">$${item.price.toFixed(2)}</td>
            <td style="padding:12px; border-bottom:1px solid var(--border); text-align:right; font-weight:700; color:var(--accent);">$${item.total.toFixed(2)}</td>
            <td style="padding:12px; border-bottom:1px solid var(--border); text-align:center; color:var(--text-muted); font-size:0.85rem;">${formattedDate}</td>
        `;
        tbody.appendChild(tr);
    });
}

window.loadSalesHistoryList = async function() {
    const dateFilter = document.getElementById('salesHistoryDateFilter');
    const dateVal = dateFilter ? dateFilter.value : '';
    
    let url = `${API_BASE}/api/sales-items?_=${Date.now()}`;
    if (dateVal) {
        url += `&date=${dateVal}`;
    }
    
    try {
        const res = await fetch(url);
        if (res.ok) {
            const items = await res.json();
            renderSalesItemsTable(items);
        } else {
            showToast("Failed to fetch sales history", "error");
        }
    } catch (e) {
        console.error("Failed to load sales items", e);
    }
};

window.loadSalesHistoryByDate = function() {
    window.loadSalesHistoryList();
};

window.submitSalesEntry = async function() {
    const select = document.getElementById('salesProductSelect');
    const qtyInput = document.getElementById('salesQuantityInput');
    const priceInput = document.getElementById('salesPriceInput');
    if (!select || !qtyInput || !priceInput) return;

    if (!select.value) {
        showToast("Please select an item", "error");
        return;
    }

    const [type, idStr] = select.value.split('-');
    const itemId = parseInt(idStr);
    const qty = parseFloat(qtyInput.value);
    const price = parseFloat(priceInput.value);

    if (!qty || qty <= 0) {
        showToast("Please enter a valid quantity", "error");
        return;
    }
    if (isNaN(price) || price <= 0) {
        showToast("Please enter a valid price", "error");
        return;
    }

    let payloadItem = null;

    if (type === 'prod') {
        const product = allProducts.find(p => p.id === itemId);
        if (!product) return;

        // Check stock if tracked
        if (product.isStockTracked && qty > product.stock) {
            showToast(`Insufficient stock! Current stock: ${product.stock}`, "error");
            return;
        }

        const uomSelect = document.getElementById('salesUomSelect');
        const selectedUom = (product.bigUnit && product.smallUnit && uomSelect) ? uomSelect.value : (product.unitOfMeasure || 'pcs');

        payloadItem = {
            id: itemId,
            name: product.name,
            price: price,
            qty: qty,
            itemType: "Part",
            unitOfMeasure: selectedUom
        };
    } else if (type === 'rec') {
        const recipe = allRecipes.find(r => r.id === itemId);
        if (!recipe) return;

        payloadItem = {
            id: itemId,
            name: recipe.name,
            price: price,
            qty: qty,
            itemType: "Recipe",
            recipeId: itemId,
            unitOfMeasure: ""
        };
    }

    if (!payloadItem) return;

    const payload = {
        items: [payloadItem]
    };

    try {
        const res = await fetch(`${API_BASE}/api/checkout`, {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify(payload)
        });

        if (res.ok) {
            showToast("Sale recorded successfully!", "success");
            
            // Re-fetch products to update cached stock count
            await fetchInventory();
            
            // Reload the sales tab dropdown and table
            await loadSalesTab();
        } else {
            const errText = await res.text();
            showToast("Checkout failed: " + errText, "error");
        }
    } catch (e) {
        showToast("Connection error", "error");
    }
};

window.clearSalesHistory = function() {
    showDeleteConfirm(
        "Clear Sales History",
        "Are you sure you want to clear all sales history and transaction logs? This action cannot be undone.",
        async () => {
            try {
                const res = await fetch(`${API_BASE}/api/clear-reports`, { method: 'POST' });
                if (res.ok) {
                    showToast("Sales history cleared successfully!", "success");
                    await fetchInventory();
                    await loadSalesTab();
                } else {
                    showToast("Failed to clear sales history", "error");
                }
            } catch (e) {
                showToast("Connection error", "error");
            }
        },
        "Clear"
    );
};

// ─── STOCK MENU MANAGEMENT ───────────────────────────────────────────
function getTodayDateString() {
    const d = new Date();
    const year = d.getFullYear();
    const month = String(d.getMonth() + 1).padStart(2, '0');
    const day = String(d.getDate()).padStart(2, '0');
    return `${year}-${month}-${day}`;
}

window.loadStockTab = function() {
    const dateInput = document.getElementById('stockReportDate');
    if (dateInput && !dateInput.value) {
        dateInput.value = getTodayDateString();
    }
    loadStockTabList();
    loadStockReport();
};

window.loadStockTabList = function() {
    const container = document.getElementById('stockTabCardsContainer');
    if (!container) return;
    container.innerHTML = '';

    const query = document.getElementById('stockTabSearch')?.value.toLowerCase() || '';
    
    const filtered = allProducts.filter(p => {
        return !query || 
            (p.name && p.name.toLowerCase().includes(query)) || 
            (p.sku && p.sku.toLowerCase().includes(query)) || 
            (p.barcode && p.barcode.toLowerCase().includes(query));
    });

    if (filtered.length === 0) {
        container.innerHTML = `<div style="text-align:center; padding:30px; color:var(--text-muted); font-weight:600; font-size:0.95rem;">No products found.</div>`;
        return;
    }

    filtered.forEach(p => {
        const card = document.createElement('div');
        card.className = 'glass-card';
        card.style.padding = '14px';
        card.style.display = 'flex';
        card.style.justifyContent = 'space-between';
        card.style.alignItems = 'center';
        card.style.border = '1px solid rgba(255,255,255,0.08)';
        card.style.background = 'rgba(255,255,255,0.03)';
        card.style.borderRadius = '10px';
        
        const unitToShow = p.bigUnit || p.unitOfMeasure || 'pcs';
        const stockInBigUnit = p.stock;
        const stockVal = stockInBigUnit.toFixed(2).replace(/\.00$/, '');

        const hasSmallUnit = p.smallUnit && p.conversionValue > 0;
        const unitBadge = hasSmallUnit ? 
            `<span id="quickAdjustUnit_${p.id}" data-unit-type="big" onclick="toggleQuickAdjustUnit(${p.id}, '${unitToShow}', '${p.smallUnit}')" style="font-size:0.85rem; color:var(--accent); font-weight:700; cursor:pointer; border-bottom:1px dashed var(--accent); max-width:60px; overflow:hidden; text-overflow:ellipsis; white-space:nowrap; transition:all 0.2s;">${unitToShow}</span>` :
            `<span style="font-size:0.85rem; color:var(--text-muted); font-weight:600; max-width:60px; overflow:hidden; text-overflow:ellipsis; white-space:nowrap;">${unitToShow}</span>`;

        card.innerHTML = `
            <div>
                <div style="font-weight:bold; font-size:1rem; color:var(--text-main);">${p.name}</div>
                <div style="font-size:0.85rem; color:var(--text-muted); margin-top:4px;">
                    Current Stock: <span style="font-weight:bold; color:${p.stock === 0 ? 'var(--danger)' : 'var(--accent)'};">${stockVal} ${unitToShow}</span>
                </div>
            </div>
            <div style="display:flex; align-items:center; gap:8px;">
                <button onclick="adjustStockItemWithInput(${p.id}, -1)" style="width:36px; height:36px; border-radius:8px; border:none; background:rgba(239, 68, 68, 0.2); color:#ef4444; font-weight:bold; font-size:1.2rem; cursor:pointer; display:flex; align-items:center; justify-content:center;">-</button>
                <div style="display:flex; align-items:center; justify-content:center; gap:4px; background:rgba(0,0,0,0.15); padding:2px 8px; border-radius:6px; border:1px solid rgba(255,255,255,0.1); width:120px; box-sizing:border-box;">
                    <input type="number" id="quickAdjustInput_${p.id}" value="1" min="0.001" step="any" style="width:40px; border:none; background:transparent; color:white; text-align:center; font-size:0.9rem; font-weight:bold; outline:none;" />
                    ${unitBadge}
                </div>
                <button onclick="adjustStockItemWithInput(${p.id}, 1)" style="width:36px; height:36px; border-radius:8px; border:none; background:rgba(16, 185, 129, 0.2); color:#10b981; font-weight:bold; font-size:1.2rem; cursor:pointer; display:flex; align-items:center; justify-content:center;">+</button>
            </div>
        `;
        container.appendChild(card);
    });
};

window.toggleQuickAdjustUnit = function(productId, bigUnit, smallUnit) {
    if (!smallUnit) return;
    const badge = document.getElementById(`quickAdjustUnit_${productId}`);
    if (!badge) return;
    
    const current = badge.getAttribute('data-unit-type');
    if (current === 'big') {
        badge.setAttribute('data-unit-type', 'small');
        badge.innerText = smallUnit;
        badge.style.color = '#f59e0b'; // amber color for small unit
        badge.style.borderBottomColor = '#f59e0b';
    } else {
        badge.setAttribute('data-unit-type', 'big');
        badge.innerText = bigUnit;
        badge.style.color = 'var(--accent)';
        badge.style.borderBottomColor = 'var(--accent)';
    }
};

window.filterStockTabList = function() {
    loadStockTabList();
};

window.adjustStockItemWithInput = async function(productId, direction) {
    const input = document.getElementById(`quickAdjustInput_${productId}`);
    if (!input) return;
    const value = parseFloat(input.value);
    if (isNaN(value) || value <= 0) {
        showToast("Please enter a valid positive number", "error");
        return;
    }
    
    const p = allProducts.find(x => x.id === productId);
    if (!p) return;

    const badge = document.getElementById(`quickAdjustUnit_${productId}`);
    const unitType = badge ? badge.getAttribute('data-unit-type') : 'big';

    let delta = direction * value;
    
    // Scale down small unit to big unit base first if selected
    if (unitType === 'small' && p.conversionValue > 0) {
        delta = delta / p.conversionValue;
    }
    
    // Scale delta (Big Unit adjustment) for database update
    const dbChange = delta;
    
    await adjustStockItem(productId, dbChange);
};

window.adjustStockItem = async function(productId, delta) {
    try {
        const res = await fetch(`${API_BASE}/api/adjust-stock`, {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify({ partId: productId, change: delta, reason: 'Daily Quick Adjustment' })
        });

        if (res.ok) {
            showToast("Stock updated!", "success");
            await fetchInventory();
            loadStockTabList();
            loadStockReport();
        } else {
            showToast("Failed to update stock", "error");
        }
    } catch (e) {
        showToast("Connection error", "error");
    }
};

window.loadStockReport = async function() {
    const container = document.getElementById('stockReportCardsContainer');
    if (!container) return;
    container.innerHTML = '';

    const selectedDate = document.getElementById('stockReportDate').value;
    if (!selectedDate) return;

    try {
        const res = await fetch(`${API_BASE}/api/stock-transactions?date=${selectedDate}&_=${Date.now()}`);
        if (res.ok) {
            const list = await res.json();
            if (list.length === 0) {
                container.innerHTML = `<div style="text-align:center; padding:30px; color:var(--text-muted); font-weight:600; font-size:0.95rem;">No stock movements recorded for this day.</div>`;
                return;
            }

            list.forEach(tx => {
                const card = document.createElement('div');
                card.className = 'glass-card';
                card.style.padding = '14px';
                card.style.display = 'flex';
                card.style.justifyContent = 'space-between';
                card.style.alignItems = 'center';
                card.style.border = '1px solid rgba(255,255,255,0.08)';
                card.style.background = 'rgba(255,255,255,0.03)';
                card.style.borderRadius = '10px';
                
                let timeStr = tx.time;
                if (tx.time && tx.time.includes(' ')) {
                    timeStr = tx.time.split(' ')[1].substring(0, 5);
                } else if (tx.time && tx.time.includes('T')) {
                    timeStr = tx.time.split('T')[1].substring(0, 5);
                }

                // Dynamic unit lookup from allProducts array
                const p = allProducts.find(x => x.name === tx.item);
                const unitToShow = p ? (p.bigUnit || p.unitOfMeasure || 'pcs') : 'pcs';

                // Try to extract the quantity change value from description
                let qty = 0;
                let hasQty = false;
                let match = tx.desc.match(/by\s+(-?\d+(\.\d+)?)/i);
                if (match) {
                    qty = parseFloat(match[1]);
                    hasQty = true;
                } else {
                    match = tx.desc.match(/(?:New\s+)?Qty:\s*(-?\d+(\.\d+)?)/i);
                    if (match) {
                        qty = parseFloat(match[1]);
                        hasQty = true;
                    } else {
                        match = tx.desc.match(/Deducted\s+(-?\d+(\.\d+)?)/i);
                        if (match) {
                            qty = parseFloat(match[1]);
                            hasQty = true;
                        }
                    }
                }

                let badgeBg = 'rgba(255, 255, 255, 0.1)';
                let badgeColor = '#fff';
                let actionText = 'EDIT';
                
                if (tx.action === 'ADJUST_IN' || tx.action === 'STOCK_ADD') {
                    badgeBg = 'rgba(16, 185, 129, 0.15)';
                    badgeColor = '#10b981';
                    actionText = 'ADDED';
                } else if (tx.action === 'ADJUST_OUT') {
                    badgeBg = 'rgba(239, 68, 68, 0.15)';
                    badgeColor = '#ef4444';
                    actionText = 'REMOVED';
                } else if (tx.action === 'STOCK_EDIT') {
                    badgeBg = 'rgba(59, 130, 246, 0.15)';
                    badgeColor = '#3b82f6';
                    actionText = 'EDIT';
                } else if (tx.action === 'STOCK_DEDUCT' || tx.action === 'DEDUCT' || tx.action === 'SALE') {
                    badgeBg = 'rgba(239, 68, 68, 0.15)';
                    badgeColor = '#ef4444';
                    actionText = 'DEDUCTED';
                }

                let qtyStr = '';
                if (hasQty) {
                    if ((actionText === 'REMOVED' || actionText === 'DEDUCTED') && qty > 0) {
                        qtyStr = `-${qty.toFixed(2).replace(/\.00$/, '')}`;
                    } else if (actionText === 'ADDED' && qty > 0) {
                        qtyStr = `+${qty.toFixed(2).replace(/\.00$/, '')}`;
                    } else {
                        qtyStr = qty > 0 ? `+${qty.toFixed(2).replace(/\.00$/, '')}` : qty.toFixed(2).replace(/\.00$/, '');
                    }
                } else {
                    qtyStr = '1';
                }

                card.innerHTML = `
                    <div>
                        <div style="font-size:0.8rem; color:var(--text-muted); font-weight:600; margin-bottom:4px;">${timeStr}</div>
                        <div style="font-weight:bold; font-size:0.95rem; color:var(--text-main);">${tx.item}</div>
                    </div>
                    <div style="text-align:right;">
                        <div style="font-size:1rem; font-weight:bold; color:${badgeColor};">${qtyStr} ${unitToShow}</div>
                        <span style="display:inline-block; margin-top:4px; padding:2px 6px; border-radius:4px; font-size:0.7rem; font-weight:bold; background:${badgeBg}; color:${badgeColor};">${actionText}</span>
                    </div>
                `;
                container.appendChild(card);
            });
        } else {
            container.innerHTML = `<div style="text-align:center; padding:30px; color:var(--danger); font-weight:600; font-size:0.95rem;">Failed to load movements.</div>`;
        }
    } catch (e) {
        container.innerHTML = `<div style="text-align:center; padding:30px; color:var(--danger); font-weight:600; font-size:0.95rem;">Connection error.</div>`;
    }
};

// ─── ICON AND IMAGE PICKER LOGIC ──────────────────────────────────────────
const PRESET_ICONS = [
    // Seafood
    '🐟', '🐠', '🐡', '🦈', '🐙', '🐚', '🦀', '🦞', '🦐', '🦑',
    // Meals
    '🍲', '🥗', '🍱', '🍛', '🍣', '🍙', '🍘', '🍝', '🍜', '🍢', '🍤', '🍳', '🥞', '🥩', '🍗', '🌭', '🍔', '🍟', '🍕',
    // Groceries / Vegetables
    '🥚', '🥛', '🧈', '🧅', '🥔', '🥕', '🌽', '🌶️', '🫑', '🥒', '🥬', '🥦', '🧄', '🍄', '🍋', '🍌', '🍎', '🍐', '🍑', '🍒', '🍓', '🫐', '🍇', '🥑',
    // Drinks / Sweets
    '🥤', '🧃', '🧉', '🍵', '☕', '🍺', '🍷', '🍧', '🍨', '🍦', '🍰', '🧁', '🍮', '🍩', '🍪',
    // General
    '📦', '🏷️', '🍽️', '🛒', '🛍️', '✨'
];

window.openIconPickerFor = function(target) {
    iconPickerTarget = target;
    
    // Set title and labels according to page language
    document.getElementById('iconPickerModal').querySelectorAll('[data-i18n]').forEach(el => {
        const key = el.getAttribute('data-i18n');
        el.textContent = t(key);
    });

    const grid = document.getElementById('presetIconsGrid');
    if (grid && grid.children.length === 0) {
        PRESET_ICONS.forEach(emoji => {
            const btn = document.createElement('button');
            btn.type = 'button';
            btn.style.cssText = 'background: rgba(255,255,255,0.05); border: 1px solid var(--border-color); border-radius: 8px; font-size: 1.8rem; cursor: pointer; aspect-ratio: 1; display: flex; align-items: center; justify-content: center; padding: 5px; transition: all 0.2s;';
            btn.innerText = emoji;
            btn.onclick = () => selectPresetIcon(emoji);
            
            // Hover effect
            btn.onmouseover = () => btn.style.background = 'rgba(255,255,255,0.15)';
            btn.onmouseout = () => btn.style.background = 'rgba(255,255,255,0.05)';
            
            grid.appendChild(btn);
        });
    }

    document.getElementById('iconPickerModal').classList.remove('hidden');
};

window.closeIconPickerModal = function() {
    document.getElementById('iconPickerModal').classList.add('hidden');
};

window.triggerDeviceGallery = function() {
    document.getElementById('iconFileInput').click();
};

function selectPresetIcon(emoji) {
    if (iconPickerTarget === 'product') {
        selectedProductIcon = emoji;
        updateIconPreview('product', emoji);
    } else {
        selectedRecipeIcon = emoji;
        updateIconPreview('recipe', emoji);
    }
    closeIconPickerModal();
}

function isImagePath(val) {
    if (!val) return false;
    if (val.startsWith('data:') || val.startsWith('http:') || val.startsWith('https:') || val.includes('/') || val.includes('\\')) {
        return true;
    }
    const lower = val.toLowerCase();
    return lower.includes('.png') || lower.includes('.jpg') || lower.includes('.jpeg') || 
           lower.includes('.gif') || lower.includes('.svg') || lower.includes('.webp') || lower.includes('.ico');
}

function updateIconPreview(type, val) {
    const previewDiv = document.getElementById(type === 'product' ? 'productIconPreview' : 'recipeIconPreview');
    if (!previewDiv) return;
    if (val && isImagePath(val)) {
        previewDiv.innerHTML = `<img src="${val}" style="width: 100%; height: 100%; object-fit: cover; border-radius: 10px;">`;
    } else {
        previewDiv.innerHTML = val || (type === 'product' ? '📦' : '🍲');
    }
}

window.confirmModalBulkImport = async function() {
    if (pendingImportType === 'sales') {
        await confirmSalesImport();
    } else {
        await confirmImport();
    }
    document.getElementById('importPreviewModal').classList.add('hidden');
};

window.closeImportPreviewModal = function() {
    const modal = document.getElementById('importPreviewModal');
    if (modal) {
        modal.classList.add('hidden');
    }
    
    // Clear file inputs so same file can be selected again
    const f1 = document.getElementById('importFile');
    if (f1) f1.value = '';
    const f2 = document.getElementById('recipeImportFile');
    if (f2) f2.value = '';
    const f3 = document.getElementById('salesImportFile');
    if (f3) f3.value = '';
};

let pendingExportCallback = null;

window.showExportFormatModal = function(exportCallback) {
    pendingExportCallback = exportCallback;
    document.getElementById('exportFormatModal').classList.remove('hidden');
};

window.closeExportFormatModal = function() {
    pendingExportCallback = null;
    document.getElementById('exportFormatModal').classList.add('hidden');
};

// Bind format selection buttons
setTimeout(() => {
    const btnCSV = document.getElementById('btnExportCSV');
    const btnExcel = document.getElementById('btnExportExcel');
    if (btnCSV) {
        btnCSV.onclick = function() {
            if (pendingExportCallback) pendingExportCallback('csv');
            closeExportFormatModal();
        };
    }
    if (btnExcel) {
        btnExcel.onclick = function() {
            if (pendingExportCallback) pendingExportCallback('excel');
            closeExportFormatModal();
        };
    }
}, 100);

// ─── USER MANAGEMENT SYSTEM ───────────────────────────────────────────
window.updateUIForRole = function() {
    const loggedIn = sessionStorage.getItem('pos_loggedIn') === 'true';
    const userMenu = document.getElementById('userMenuContainer');
    if (userMenu) {
        userMenu.style.display = loggedIn ? 'block' : 'none';
    }
    const navUsername = document.getElementById('navBarUsername');
    const username = (sessionStorage.getItem('pos_username') || '').toLowerCase().trim();
    const role = (sessionStorage.getItem('pos_role') || '').toLowerCase().trim();

    if (navUsername) {
        navUsername.innerText = sessionStorage.getItem('pos_username') || 'User';
    }

    // Tab buttons references
    const btnInventory = document.getElementById('tabBtnInventory');
    const btnRecipes = document.getElementById('tabBtnRecipes');
    const btnProduction = document.getElementById('tabBtnProduction');
    const btnStock = document.getElementById('tabBtnStock');
    const btnSales = document.getElementById('tabBtnSales');
    const btnReports = document.getElementById('tabBtnReports');
    const btnInfo = document.getElementById('tabBtnInfo');

    if (loggedIn) {
        const isSuperAdmin = (username === 'softio.admin');
        const isProductionUser = (username === 'production' || role === 'production');

        // Production page: only visible to it and to the super admin
        if (btnProduction) {
            btnProduction.style.display = (isSuperAdmin || isProductionUser) ? 'inline-block' : 'none';
        }

        // For "this user" (ProductionUser), it will not show sales entry page or stock menu page or reports. Rest are visible.
        if (isProductionUser) {
            if (btnSales) btnSales.style.display = 'none';
            if (btnStock) btnStock.style.display = 'none';
            if (btnReports) btnReports.style.display = 'none';
            
            if (btnInventory) btnInventory.style.display = 'inline-block';
            if (btnRecipes) btnRecipes.style.display = 'inline-block';
            if (btnInfo) btnInfo.style.display = 'inline-block';
        } else {
            // Restore standard visibility for other users
            if (btnSales) btnSales.style.display = 'inline-block';
            if (btnStock) btnStock.style.display = 'inline-block';
            if (btnReports) btnReports.style.display = 'inline-block';
            
            if (btnInventory) btnInventory.style.display = 'inline-block';
            if (btnRecipes) btnRecipes.style.display = 'inline-block';
            if (btnInfo) btnInfo.style.display = 'inline-block';
        }
    } else {
        // Not logged in (hide everything)
        [btnInventory, btnRecipes, btnProduction, btnStock, btnSales, btnReports, btnInfo].forEach(btn => {
            if (btn) btn.style.display = 'none';
        });
    }
};

window.loadUsersTab = async function() {
    const tableBody = document.getElementById('usersTableBody');
    if (!tableBody) return;
    tableBody.innerHTML = '<tr><td colspan="6" style="text-align:center; padding:20px; color:var(--text-muted);">Loading users...</td></tr>';
    
    try {
        const res = await fetch(`${API_BASE}/api/users?_=${Date.now()}`);
        if (res.status === 403) {
            tableBody.innerHTML = '<tr><td colspan="6" style="text-align:center; padding:20px; color:var(--danger);">Unauthorized. Only administrators can view this page.</td></tr>';
            return;
        }
        if (!res.ok) throw new Error("Failed to fetch users");
        
        const users = await res.json();
        if (users.length === 0) {
            tableBody.innerHTML = '<tr><td colspan="6" style="text-align:center; padding:20px; color:var(--text-muted);">No users found.</td></tr>';
            return;
        }
        
        let html = '';
        users.forEach(u => {
            const statusText = u.isActive === 1 ? 'Active' : 'Inactive';
            const statusClass = u.isActive === 1 ? 'text-success' : 'text-danger';
            
            const currentUsername = sessionStorage.getItem('pos_username') || '';
            const isSelf = u.username.toLowerCase() === currentUsername.toLowerCase();
            const isSuperAdmin = u.username.toLowerCase() === 'softio.admin';
            const canDelete = !isSelf && !isSuperAdmin;
            
            const userStr = JSON.stringify(u).replace(/"/g, '&quot;');
            
            html += `
                <tr style="border-bottom:1px solid rgba(0,0,0,0.05);">
                    <td style="padding:12px; color:black; font-weight:600;">${escapeHtml(u.username)}</td>
                    <td style="padding:12px; color:black;">${escapeHtml(u.fullName || '')}</td>
                    <td style="padding:12px; text-align:center;"><span class="badge" style="background:rgba(0,0,0,0.05); padding:4px 8px; border-radius:4px; font-size:0.8rem; color:var(--accent); font-weight:600;">${escapeHtml(u.role)}</span></td>
                    <td style="padding:12px; text-align:center;"><span style="font-weight:bold; color:${u.isActive === 1 ? '#10b981' : '#ef4444'}">${statusText}</span></td>
                    <td style="padding:12px; text-align:center; color:black;">${escapeHtml(u.dateCreated || '')}</td>
                    <td style="padding:12px; text-align:right;">
                        <button class="btn-primary" onclick="openEditUserModal('${userStr}')" style="width:auto; padding:4px 8px; font-size:0.75rem; background:var(--accent) !important; color:white; border-radius:4px; margin-right:5px; height:auto;">Edit</button>
                        ${canDelete ? `<button class="btn-clear" onclick="deleteUser(${u.id})" style="width:auto; padding:4px 8px; font-size:0.75rem; background:#ef4444; color:white; border:none; border-radius:4px; cursor:pointer; font-weight:bold; height:auto;">Delete</button>` : ''}
                    </td>
                </tr>
            `;
        });
        tableBody.innerHTML = html;
    } catch (e) {
        tableBody.innerHTML = '<tr><td colspan="6" style="text-align:center; padding:20px; color:var(--danger);">Error loading users.</td></tr>';
        showToast("Error loading users", "error");
    }
};

window.openAddUserModal = function() {
    document.getElementById('userModalTitle').innerText = 'Add User';
    document.getElementById('userEditId').value = '';
    document.getElementById('userUsername').value = '';
    document.getElementById('userUsername').removeAttribute('disabled');
    document.getElementById('userPassword').value = '';
    document.getElementById('userFullName').value = '';
    document.getElementById('userRole').value = 'Staff';
    document.getElementById('userStatus').value = '1';
    
    document.getElementById('userModal').classList.remove('hidden');
};

window.openEditUserModal = function(userStr) {
    const u = JSON.parse(userStr.replace(/&quot;/g, '"'));
    
    document.getElementById('userModalTitle').innerText = 'Edit User';
    document.getElementById('userEditId').value = u.id;
    document.getElementById('userUsername').value = u.username;
    if (u.username.toLowerCase() === 'softio.admin') {
        document.getElementById('userUsername').setAttribute('disabled', 'true');
    } else {
        document.getElementById('userUsername').removeAttribute('disabled');
    }
    document.getElementById('userPassword').value = u.password;
    document.getElementById('userFullName').value = u.fullName || '';
    document.getElementById('userRole').value = u.role || 'Staff';
    document.getElementById('userStatus').value = u.isActive.toString();
    
    document.getElementById('userModal').classList.remove('hidden');
};

window.closeUserModal = function() {
    document.getElementById('userModal').classList.add('hidden');
};

window.saveUser = async function() {
    const idVal = document.getElementById('userEditId').value;
    const username = document.getElementById('userUsername').value.trim();
    const password = document.getElementById('userPassword').value.trim();
    const fullName = document.getElementById('userFullName').value.trim();
    const role = document.getElementById('userRole').value;
    const isActive = parseInt(document.getElementById('userStatus').value);
    
    if (!username || !password) {
        showToast("Username and Password are required", "error");
        return;
    }
    
    const payload = {
        username: username,
        password: password,
        fullName: fullName,
        role: role,
        isActive: isActive
    };
    if (idVal) {
        payload.id = parseInt(idVal);
    }
    
    try {
        const res = await fetch(`${API_BASE}/api/users`, {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify(payload)
        });
        
        if (res.ok) {
            showToast("User saved successfully", "success");
            closeUserModal();
            loadUsersTab();
        } else {
            const err = await res.json();
            showToast(err.error || "Failed to save user", "error");
        }
    } catch (e) {
        showToast("Connection error", "error");
    }
};

window.deleteUser = async function(id) {
    if (!confirm("Are you sure you want to delete this user?")) return;
    
    try {
        const res = await fetch(`${API_BASE}/api/users/${id}`, {
            method: 'DELETE'
        });
        
        if (res.ok) {
            showToast("User deleted successfully", "success");
            loadUsersTab();
        } else {
            const err = await res.text();
            showToast(err || "Failed to delete user", "error");
        }
    } catch (e) {
        showToast("Connection error", "error");
    }
};

function escapeHtml(text) {
    if (!text) return '';
    return text
        .replace(/&/g, "&amp;")
        .replace(/</g, "&lt;")
        .replace(/>/g, "&gt;")
        .replace(/"/g, "&quot;")
        .replace(/'/g, "&#039;");
}


// Toggle User Dropdown Menu (No-op since dropdown is removed)
window.toggleUserMenu = function(event) {
    event.stopPropagation();
};

// Implement Web User Profile & Activities
window.goBackFromUserProfile = function() {
    const backTo = window.lastActiveTabId || 'inventory';
    switchTab(backTo);
};

window.triggerWebSwitchUser = function() {
    handleLock();
};

window.triggerWebLogout = function() {
    showDeleteConfirm(
        "Logout",
        "Are you sure you want to logout?",
        () => {
            handleLogout();
        },
        "Logout"
    );
};

window.loadWebUserProfile = async function() {
    const username = sessionStorage.getItem('pos_username') || 'User';
    const fullName = sessionStorage.getItem('pos_user') || 'Full Name';
    const role = sessionStorage.getItem('pos_role') || 'User';
    
    document.getElementById('profileFullName').innerText = fullName;
    document.getElementById('profileUsername').innerText = '@' + username;
    document.getElementById('profileRole').innerText = role;
    
    // Set initials avatar
    const initial = fullName.charAt(0).toUpperCase() || 'U';
    document.getElementById('profileAvatar').innerText = initial;
    
    // Set Active status
    const statusBadge = document.getElementById('profileStatusBadge');
    let isActive = 1;
    try {
        const res = await fetch(`${API_BASE}/api/users?_=${Date.now()}`);
        if (res.ok) {
            const users = await res.json();
            const curr = users.find(u => u.username.toLowerCase() === username.toLowerCase());
            if (curr) isActive = curr.isActive;
        }
    } catch (e) {}

    if (isActive === 1) {
        statusBadge.innerText = "Active";
        statusBadge.style.background = "rgba(34,197,94,0.15)";
        statusBadge.style.color = "#22c55e";
    } else {
        statusBadge.innerText = "Inactive";
        statusBadge.style.background = "rgba(239,68,68,0.15)";
        statusBadge.style.color = "#ef4444";
    }

    const isAdmin = role.toLowerCase() === 'admin';
    const filterContainer = document.getElementById('webLogFilterContainer');
    const thUser = document.getElementById('thWebLogUser');
    
    if (isAdmin) {
        filterContainer.style.display = 'flex';
        thUser.style.display = '';
        await populateWebLogUserFilter();
    } else {
        filterContainer.style.display = 'none';
        thUser.style.display = 'none';
    }

    await loadWebActivityLogs();
};

async function populateWebLogUserFilter() {
    const select = document.getElementById('webLogUserFilter');
    if (!select) return;
    
    const currentSel = select.value;
    select.innerHTML = '<option value="all">All Users</option>';
    
    try {
        const res = await fetch(`${API_BASE}/api/users?_=${Date.now()}`);
        if (res.ok) {
            const users = await res.json();
            users.forEach(u => {
                const opt = document.createElement('option');
                opt.value = u.username;
                opt.innerText = u.username;
                select.appendChild(opt);
            });
        }
    } catch (e) {
        console.error("Failed to populate user filter", e);
    }
    
    if (currentSel) select.value = currentSel;
}

window.loadWebActivityLogs = async function() {
    const tbody = document.getElementById('webActivityLogBody');
    if (!tbody) return;
    
    tbody.innerHTML = '<tr><td colspan="4" style="text-align:center; padding:15px; color:black;">Loading logs...</td></tr>';
    
    const role = sessionStorage.getItem('pos_role') || 'User';
    const isAdmin = role.toLowerCase() === 'admin';
    
    let url = `${API_BASE}/api/logs?_=${Date.now()}`;
    if (isAdmin) {
        const filterVal = document.getElementById('webLogUserFilter').value;
        url += `&username=${encodeURIComponent(filterVal)}`;
    }
    
    try {
        const res = await fetch(url);
        if (!res.ok) throw new Error("Failed to fetch logs");
        
        const logs = await res.json();
        if (logs.length === 0) {
            tbody.innerHTML = `<tr><td colspan="${isAdmin ? 4 : 3}" style="text-align:center; padding:15px; color:black;">No activities logged yet.</td></tr>`;
            return;
        }
        
        let html = '';
        logs.forEach(log => {
            let dateStr = 'N/A';
            let timeStr = 'N/A';
            if (log.timestamp) {
                const d = new Date(log.timestamp);
                dateStr = d.getFullYear() + '-' + String(d.getMonth() + 1).padStart(2, '0') + '-' + String(d.getDate()).padStart(2, '0');
                timeStr = String(d.getHours()).padStart(2, '0') + ':' + String(d.getMinutes()).padStart(2, '0') + ':' + String(d.getSeconds()).padStart(2, '0');
            }
            
            html += `<tr style="border-bottom:1px solid rgba(0,0,0,0.08); color:black;">
                <td style="padding:10px;">${dateStr}</td>
                <td style="padding:10px;">${timeStr}</td>
                ${isAdmin ? `<td style="padding:10px; font-weight:600; color:var(--accent);">${escapeHtml(log.username)}</td>` : ''}
                <td style="padding:10px;">${escapeHtml(log.action)}</td>
            </tr>`;
        });
        tbody.innerHTML = html;
    } catch (e) {
        tbody.innerHTML = `<tr><td colspan="${isAdmin ? 4 : 3}" style="text-align:center; padding:15px; color:#ef4444;">Failed to load logs.</td></tr>`;
    }
};

window.exportRecipes = async function(format) {
    try {
        const headers = ["Recipe Name", "Category", "Price", "Description", "Item No", "Ingredient", "Ingredient Quantity", "Measurement Unit"];
        const rows = [];
        
        allRecipes.forEach(r => {
            const recipeName = r.name || '';
            const category = r.categoryName || 'General';
            const price = parseFloat(r.price || 0.00);
            const desc = r.description || '';
            const itemNo = r.itemNo || '';

            if (r.parts && r.parts.length > 0) {
                r.parts.forEach(p => {
                    const prod = allProducts.find(x => x.id === p.partId);
                    const prodName = prod ? prod.name : `Part #${p.partId}`;
                    const ingredientName = prodName;
                    const qty = p.qty;
                    const uom = p.unitOfMeasure || '';
                    
                    rows.push([recipeName, category, price, desc, itemNo, ingredientName, qty, uom]);
                });
            } else {
                rows.push([recipeName, category, price, desc, itemNo, '', '', '']);
            }
        });

        if (format === 'excel') {
            const worksheet = XLSX.utils.aoa_to_sheet([headers, ...rows]);
            const workbook = XLSX.utils.book_new();
            XLSX.utils.book_append_sheet(workbook, worksheet, "Recipes");
            const base64Content = XLSX.write(workbook, { bookType: 'xlsx', type: 'base64' });
            const filename = `recipes_${new Date().toISOString().slice(0,10)}.xlsx`;

            await sendFileToBackend(filename, null, base64Content);
        } else {
            const csvRows = rows.map(r => r.map(val => {
                if (typeof val === 'string') {
                    return `"${val.replace(/"/g, '""')}"`;
                }
                return val;
            }));
            const csvContent = [headers.join(','), ...csvRows.map(r => r.join(','))].join('\n');
            const filename = `recipes_${new Date().toISOString().slice(0,10)}.csv`;

            await sendFileToBackend(filename, csvContent, null);
        }
    } catch (e) {
        showToast("Error exporting recipes: " + e.message, "error");
    }
};

window.exportSales = async function(format) {
    try {
        const resSales = await fetch(`${API_BASE}/api/sales-export?_=${Date.now()}`);
        if (!resSales.ok) {
            let errorText = "Failed to fetch sales data";
            try {
                const errData = await resSales.json();
                if (errData && errData.error) errorText = errData.error;
            } catch (jsonErr) {}
            throw new Error(errorText);
        }

        const sales = await resSales.json();
        const headers = ["Item No", "Item Name", "Qty Sold", "Unit"];
        const rows = sales.map(s => {
            return [
                s.itemNo || '',
                s.itemName || '',
                s.qtySold,
                s.unit || 'pcs'
            ];
        });

        if (format === 'excel') {
            const worksheet = XLSX.utils.aoa_to_sheet([headers, ...rows]);
            const workbook = XLSX.utils.book_new();
            XLSX.utils.book_append_sheet(workbook, worksheet, "Sales");
            const base64Content = XLSX.write(workbook, { bookType: 'xlsx', type: 'base64' });
            const filename = `sales_${new Date().toISOString().slice(0,10)}.xlsx`;

            await sendFileToBackend(filename, null, base64Content);
        } else {
            const csvRows = rows.map(r => r.map(val => {
                if (typeof val === 'string') {
                    return `"${val.replace(/"/g, '""')}"`;
                }
                return val;
            }));
            const csvContent = [headers.join(','), ...csvRows.map(r => r.join(','))].join('\n');
            const filename = `sales_${new Date().toISOString().slice(0,10)}.csv`;
 
            await sendFileToBackend(filename, csvContent, null);
        }
    } catch (e) {
        showToast(`Export failed: ${e.message}`, "error");
    }
};

// ─── CUSTOM SALES PRODUCT SELECTOR ─────────────────────────────────────
window.customSalesActiveTab = 'recipes';
window.customSalesFrequencies = {};

// Position dropdown menu relative to the trigger
window.positionCustomSalesDropdown = function() {
    const trigger = document.querySelector('.custom-select-trigger');
    const menu = document.getElementById('customSalesProductMenu');
    if (!trigger || !menu || menu.classList.contains('hidden')) return;
    
    // Check if we are on a mobile device or if body has class is-mobile-layout
    const isMobile = document.body.classList.contains('is-mobile-layout') || window.innerWidth <= 768;
    
    if (isMobile) {
        // Positioned centered by CSS
        menu.style.position = '';
        menu.style.top = '';
        menu.style.left = '';
        menu.style.width = '';
    } else {
        // Desktop absolute positioning
        const rect = trigger.getBoundingClientRect();
        
        menu.style.position = 'absolute';
        menu.style.top = `${rect.bottom + window.scrollY}px`;
        menu.style.left = `${rect.left + window.scrollX}px`;
        menu.style.width = `${Math.max(rect.width, 420)}px`;
        
        // Prevent overflowing the right edge of the screen
        const menuRect = menu.getBoundingClientRect();
        if (rect.left + menuRect.width > window.innerWidth) {
            menu.style.left = `${window.innerWidth - menuRect.width - 20 + window.scrollX}px`;
        }
    }
};

// Toggle dropdown open/close
window.toggleCustomSalesDropdown = function(event) {
    if (event) event.stopPropagation();
    const menu = document.getElementById('customSalesProductMenu');
    const trigger = document.querySelector('.custom-select-trigger');
    if (!menu || !trigger) return;
    
    // Move to body if not already reparented (portal style)
    if (menu.parentNode !== document.body) {
        document.body.appendChild(menu);
    }
    
    // Handle background overlay backdrop
    let backdrop = document.getElementById('customSalesDropdownBackdrop');
    if (!backdrop) {
        backdrop = document.createElement('div');
        backdrop.id = 'customSalesDropdownBackdrop';
        backdrop.className = 'custom-select-backdrop hidden';
        backdrop.onclick = function(e) {
            window.toggleCustomSalesDropdown(e);
        };
        document.body.appendChild(backdrop);
    }
    
    const isHidden = menu.classList.contains('hidden');
    
    // Reset all triggers and dropdowns
    document.querySelectorAll('.custom-select-dropdown').forEach(m => m.classList.add('hidden'));
    document.querySelectorAll('.custom-select-trigger').forEach(t => t.classList.remove('active'));
    backdrop.classList.add('hidden');
    
    if (isHidden) {
        menu.classList.remove('hidden');
        trigger.classList.add('active');
        backdrop.classList.remove('hidden');
        
        // Dynamically compute positioning coordinates
        window.positionCustomSalesDropdown();
        
        const searchInput = document.getElementById('customSalesProductSearch');
        if (searchInput) {
            searchInput.value = '';
            searchInput.focus();
            window.refreshCustomSalesDropdown(false);
        }
    } else {
        menu.classList.add('hidden');
        trigger.classList.remove('active');
        backdrop.classList.add('hidden');
    }
};

// Switch active category tab
window.switchCustomSalesTab = function(tab) {
    window.customSalesActiveTab = tab;
    
    const tabRecipes = document.getElementById('customSalesTabRecipes');
    const tabIngredients = document.getElementById('customSalesTabIngredients');
    if (tabRecipes && tabIngredients) {
        if (tab === 'recipes') {
            tabRecipes.classList.add('active');
            tabIngredients.classList.remove('active');
        } else {
            tabRecipes.classList.remove('active');
            tabIngredients.classList.add('active');
        }
    }
    
    const panelRecipes = document.getElementById('customSalesPanelRecipes');
    const panelIngredients = document.getElementById('customSalesPanelIngredients');
    if (panelRecipes && panelIngredients) {
        if (tab === 'recipes') {
            panelRecipes.classList.remove('hidden');
            panelIngredients.classList.add('hidden');
        } else {
            panelRecipes.classList.add('hidden');
            panelIngredients.classList.remove('hidden');
        }
    }
    
    window.refreshCustomSalesDropdown(false);
};

// Handle search input filtering
window.onCustomSalesSearch = function(event) {
    window.refreshCustomSalesDropdown(false);
};

// Toggle Pin/Unpin status
window.toggleRecipePin = function(event, recipeId) {
    if (event) event.stopPropagation();
    
    let pinned = [];
    try {
        const stored = localStorage.getItem('salesPinnedRecipes');
        if (stored) pinned = JSON.parse(stored);
    } catch(e) {}
    
    const idx = pinned.indexOf(recipeId);
    if (idx > -1) {
        pinned.splice(idx, 1);
    } else {
        pinned.push(recipeId);
    }
    
    localStorage.setItem('salesPinnedRecipes', JSON.stringify(pinned));
    window.refreshCustomSalesDropdown(false);
};

// Select an item from custom dropdown
window.selectSalesItem = function(val) {
    const select = document.getElementById('salesProductSelect');
    if (!select) return;
    
    select.value = val;
    
    if (typeof window.onSalesProductChange === 'function') {
        window.onSalesProductChange();
    }
    
    const menu = document.getElementById('customSalesProductMenu');
    const trigger = document.querySelector('.custom-select-trigger');
    const backdrop = document.getElementById('customSalesDropdownBackdrop');
    if (menu) menu.classList.add('hidden');
    if (trigger) trigger.classList.remove('active');
    if (backdrop) backdrop.classList.add('hidden');
    
    window.updateCustomSalesTriggerText();
};

// Update the visible trigger display text
window.updateCustomSalesTriggerText = function() {
    const select = document.getElementById('salesProductSelect');
    const triggerText = document.getElementById('customSalesProductSelectedText');
    if (!select || !triggerText) return;
    
    if (!select.value) {
        triggerText.textContent = "Select an item...";
        return;
    }
    
    const [type, idStr] = select.value.split('-');
    const itemId = parseInt(idStr);
    
    if (type === 'prod') {
        const product = allProducts.find(p => p.id === itemId);
        if (product) {
            triggerText.textContent = `${product.name} ($${product.price.toFixed(2)})`;
        }
    } else if (type === 'rec') {
        const recipe = allRecipes.find(r => r.id === itemId);
        if (recipe) {
            triggerText.textContent = `${recipe.name} ($${recipe.price.toFixed(2)})`;
        }
    }
};

// Render dropdown list dynamically
window.refreshCustomSalesDropdown = async function(fetchFreq = true) {
    const select = document.getElementById('salesProductSelect');
    if (!select) return;
    
    window.updateCustomSalesTriggerText();
    
    if (fetchFreq) {
        try {
            const res = await fetch(`${API_BASE}/api/recipes/sales-frequency?_=${Date.now()}`);
            if (res.ok) {
                window.customSalesFrequencies = await res.json();
            }
        } catch (e) {
            console.error("Failed to fetch recipe sales frequencies:", e);
        }
    }
    
    const searchInput = document.getElementById('customSalesProductSearch');
    const query = searchInput ? searchInput.value.trim().toLowerCase() : '';
    
    let pinned = [];
    try {
        const stored = localStorage.getItem('salesPinnedRecipes');
        if (stored) pinned = JSON.parse(stored);
    } catch(e) {}
    
    if (window.customSalesActiveTab === 'recipes') {
        const quickAccessList = document.getElementById('customSalesQuickAccessList');
        const allRecipesList = document.getElementById('customSalesAllRecipesList');
        
        if (!quickAccessList || !allRecipesList) return;
        
        const activeRecipes = allRecipes.filter(r => r.status !== 'Inactive');
        
        const filteredRecipes = query 
            ? activeRecipes.filter(r => r.name.toLowerCase().includes(query))
            : activeRecipes;
            
        const sortedAllRecipes = [...filteredRecipes].sort((a, b) => a.name.localeCompare(b.name));
        
        allRecipesList.innerHTML = '';
        if (sortedAllRecipes.length === 0) {
            allRecipesList.innerHTML = '<div style="padding:12px; text-align:center; color:var(--text-muted); font-size:0.85rem;">No recipes found</div>';
        } else {
            sortedAllRecipes.forEach(r => {
                const isPinned = pinned.includes(r.id);
                const isSelected = select.value === `rec-${r.id}`;
                
                const itemDiv = document.createElement('div');
                itemDiv.className = `custom-select-item ${isSelected ? 'active' : ''}`;
                itemDiv.onclick = () => window.selectSalesItem(`rec-${r.id}`);
                
                itemDiv.innerHTML = `
                    <div class="item-details">
                        <span class="item-name">${r.name}</span>
                        <span class="item-price">$${r.price.toFixed(2)}</span>
                    </div>
                    <span class="star-btn ${isPinned ? 'pinned' : ''}" onclick="window.toggleRecipePin(event, ${r.id})">
                        ${isPinned ? '★' : '☆'}
                    </span>
                `;
                allRecipesList.appendChild(itemDiv);
            });
        }
        
        const rankedRecipes = activeRecipes.map(r => {
            const isPinned = pinned.includes(r.id);
            const frequency = window.customSalesFrequencies[r.id] || 0;
            return { r, isPinned, frequency };
        });
        
        rankedRecipes.sort((a, b) => {
            if (a.isPinned && !b.isPinned) return -1;
            if (!a.isPinned && b.isPinned) return 1;
            if (b.frequency !== a.frequency) return b.frequency - a.frequency;
            return a.r.name.localeCompare(b.r.name);
        });
        
        const quickAccessItems = rankedRecipes.slice(0, 8);
        
        quickAccessList.innerHTML = '';
        if (quickAccessItems.length === 0) {
            quickAccessList.innerHTML = '<div style="grid-column: span 2; padding:12px; text-align:center; color:var(--text-muted); font-size:0.85rem;">No recipes available</div>';
        } else {
            quickAccessItems.forEach(({ r, isPinned }) => {
                const isSelected = select.value === `rec-${r.id}`;
                const itemDiv = document.createElement('div');
                itemDiv.className = `custom-quick-access-item ${isSelected ? 'active' : ''}`;
                itemDiv.onclick = () => window.selectSalesItem(`rec-${r.id}`);
                
                itemDiv.innerHTML = `
                    <span class="quick-access-text">${r.name} ($${r.price.toFixed(2)})</span>
                    <span class="star-btn" onclick="window.toggleRecipePin(event, ${r.id})">
                        ${isPinned ? '★' : '☆'}
                    </span>
                `;
                quickAccessList.appendChild(itemDiv);
            });
        }
    } else {
        const ingredientsList = document.getElementById('customSalesIngredientsList');
        if (!ingredientsList) return;
        
        const activeIngredients = allProducts.filter(p => p.status !== 'Inactive');
        
        const filteredIngredients = query
            ? activeIngredients.filter(p => p.name.toLowerCase().includes(query))
            : activeIngredients;
            
        const sortedIngredients = [...filteredIngredients].sort((a, b) => a.name.localeCompare(b.name));
        
        ingredientsList.innerHTML = '';
        if (sortedIngredients.length === 0) {
            ingredientsList.innerHTML = '<div style="padding:12px; text-align:center; color:var(--text-muted); font-size:0.85rem;">No ingredients found</div>';
        } else {
            sortedIngredients.forEach(p => {
                const isSelected = select.value === `prod-${p.id}`;
                
                const itemDiv = document.createElement('div');
                itemDiv.className = `custom-select-item ${isSelected ? 'active' : ''}`;
                itemDiv.onclick = () => window.selectSalesItem(`prod-${p.id}`);
                
                itemDiv.innerHTML = `
                    <div class="item-details" style="margin-right: 0;">
                        <span class="item-name">${p.name}</span>
                        <span class="item-price">$${p.price.toFixed(2)}</span>
                    </div>
                `;
                ingredientsList.appendChild(itemDiv);
            });
        }
    }
};

// Global click handler to close dropdown
document.addEventListener('click', function(event) {
    const dropdown = document.getElementById('customSalesProductDropdown');
    const menu = document.getElementById('customSalesProductMenu');
    const backdrop = document.getElementById('customSalesDropdownBackdrop');
    
    if ((dropdown && dropdown.contains(event.target)) || (menu && menu.contains(event.target))) {
        return;
    }
    
    if (menu && !menu.classList.contains('hidden')) {
        menu.classList.add('hidden');
        const trigger = document.querySelector('.custom-select-trigger');
        if (trigger) trigger.classList.remove('active');
        if (backdrop) backdrop.classList.add('hidden');
    }
});

// Automatically close dropdown on page/container scrolls
document.addEventListener('scroll', function(event) {
    const menu = document.getElementById('customSalesProductMenu');
    const backdrop = document.getElementById('customSalesDropdownBackdrop');
    if (menu && menu.contains(event.target)) return;
    
    if (menu && !menu.classList.contains('hidden')) {
        menu.classList.add('hidden');
        const trigger = document.querySelector('.custom-select-trigger');
        if (trigger) trigger.classList.remove('active');
        if (backdrop) backdrop.classList.add('hidden');
    }
}, true);

// Reposition dropdown on window resize
window.addEventListener('resize', function() {
    window.positionCustomSalesDropdown();
});

// ─── INFO TAB & FACTORY RESET FLOW ────────────────────────────────────
window.loadInfoTab = async function() {
    const container = document.getElementById('infoLicenseDetails');
    if (!container) return;
    
    container.innerHTML = '<div style="color:var(--text-muted);">Loading license details...</div>';
    
    try {
        const res = await fetch(`${API_BASE}/api/license-info?_=${Date.now()}`);
        if (res.ok) {
            const data = await res.json();
            container.innerHTML = `
                <p style="margin:5px 0;"><strong>License Key:</strong> <span style="font-family:monospace; color:var(--accent); font-weight:700;">${data.key}</span></p>
                <p style="margin:5px 0;"><strong>License Type:</strong> ${data.licenseType}</p>
                <p style="margin:5px 0;"><strong>Customer Name:</strong> ${data.customerName}</p>
                <p style="margin:5px 0;"><strong>Activation Date:</strong> ${data.activationDate || 'N/A'}</p>
                <p style="margin:5px 0;"><strong>Expiration Date:</strong> ${data.expirationDate}</p>
                <p style="margin:5px 0;"><strong>Days Remaining:</strong> <span class="badge" style="background:${data.daysRemaining > 30 ? 'rgba(16,185,129,0.1)' : 'rgba(239,68,68,0.1)'}; color:${data.daysRemaining > 30 ? '#10b981' : '#ef4444'}; font-weight:700; padding:2px 8px; border-radius:4px;">${data.daysRemaining} days</span></p>
                <p style="margin:5px 0;"><strong>Status:</strong> <span style="font-weight:700; color:${data.status.includes('Valid') ? '#10b981' : '#ef4444'};">${data.status}</span></p>
            `;
        } else {
            container.innerHTML = '<div style="color:#ef4444; font-weight:600;">Failed to load license details</div>';
        }
    } catch(e) {
        container.innerHTML = '<div style="color:#ef4444; font-weight:600;">Connection error loading license details</div>';
    }
};

window.triggerFactoryResetFlow = function() {
    const role = sessionStorage.getItem('pos_role') || 'Staff';
    const isAdmin = role.toLowerCase() === 'admin';
    
    if (isAdmin) {
        showDeleteConfirm(
            "Factory Reset",
            "This will permanently delete all data in the application. This action cannot be undone.\n\nAre you sure you want to continue?",
            () => {
                promptAdminPassword(sessionStorage.getItem('pos_username') || 'Admin');
            },
            "Continue"
        );
    } else {
        promptAdminAuthorization();
    }
};

function promptAdminPassword(username) {
    const overlay = document.createElement('div');
    overlay.className = 'overlay';
    overlay.style.zIndex = '99999';
    overlay.style.display = 'flex';
    overlay.style.alignItems = 'center';
    overlay.style.justifyContent = 'center';
    overlay.style.background = 'rgba(0, 0, 0, 0.4)';
    overlay.style.backdropFilter = 'blur(4px)';

    const card = document.createElement('div');
    card.className = 'scanner-card';
    card.style.maxWidth = '380px';
    card.style.padding = '24px';
    card.style.borderRadius = '16px';
    card.style.background = 'var(--bg-secondary)';
    card.style.border = '1px solid var(--border-color)';
    card.style.boxShadow = '0 10px 30px rgba(0,0,0,0.15)';
    card.style.textAlign = 'center';

    card.innerHTML = `
        <h3 style="margin-top:0; color:var(--text-main); font-size:1.2rem; font-weight:800;">Admin Authentication</h3>
        <p style="color:var(--text-muted); font-size:0.9rem; margin:10px 0 15px 0;">Enter password for Admin user <strong>${username}</strong> to authorize Factory Reset:</p>
        <input type="password" id="resetAdminPasswordInput" placeholder="Password" style="width:100%; padding:10px; margin-bottom:20px; background:var(--bg-card); border:1px solid var(--border-color); border-radius:6px; color:var(--text); box-sizing:border-box; outline:none;" />
        <div style="display:flex; gap:12px; justify-content:center;">
            <button class="btn-clear" id="resetAuthCancelBtn" style="flex:1; height:40px; border:1px solid var(--border-color); color:var(--text-main); border-radius:8px; font-weight:600; cursor:pointer;">Cancel</button>
            <button class="btn-primary" id="resetAuthVerifyBtn" style="flex:1; height:40px; background:var(--accent); color:white; border:none; border-radius:8px; font-weight:600; cursor:pointer;">Verify</button>
        </div>
    `;

    overlay.appendChild(card);
    document.body.appendChild(overlay);

    const closePrompt = () => {
        document.body.removeChild(overlay);
    };

    overlay.querySelector('#resetAuthCancelBtn').onclick = closePrompt;
    
    const input = card.querySelector('#resetAdminPasswordInput');
    setTimeout(() => input.focus(), 100);

    input.onkeydown = (e) => {
        if (e.key === 'Enter') {
            overlay.querySelector('#resetAuthVerifyBtn').click();
        }
    };

    overlay.querySelector('#resetAuthVerifyBtn').onclick = () => {
        const password = input.value.trim();
        if (!password) {
            showToast("Password cannot be empty", "error");
            return;
        }

        closePrompt();
        showFinalConfirmation(username, password);
    };
}

function promptAdminAuthorization() {
    const overlay = document.createElement('div');
    overlay.className = 'overlay';
    overlay.style.zIndex = '99999';
    overlay.style.display = 'flex';
    overlay.style.alignItems = 'center';
    overlay.style.justifyContent = 'center';
    overlay.style.background = 'rgba(0, 0, 0, 0.4)';
    overlay.style.backdropFilter = 'blur(4px)';

    const card = document.createElement('div');
    card.className = 'scanner-card';
    card.style.maxWidth = '380px';
    card.style.padding = '24px';
    card.style.borderRadius = '16px';
    card.style.background = 'var(--bg-secondary)';
    card.style.border = '1px solid var(--border-color)';
    card.style.boxShadow = '0 10px 30px rgba(0,0,0,0.15)';
    card.style.textAlign = 'center';

    card.innerHTML = `
        <h3 style="margin-top:0; color:#ef4444; font-size:1.2rem; font-weight:800; display:flex; align-items:center; justify-content:center; gap:6px;">
            <svg viewBox="0 0 24 24" width="18" height="18" stroke="currentColor" stroke-width="2.5" fill="none"><rect x="3" y="11" width="18" height="11" rx="2" ry="2"></rect><path d="M7 11V7a5 5 0 0 1 10 0v4"></path></svg>
            Admin Authorization Required
        </h3>
        <p style="color:var(--text-muted); font-size:0.85rem; margin:10px 0 15px 0;">Factory Reset can only be performed with Admin authorization. Require an Admin to login below:</p>
        
        <input type="text" id="authAdminUsernameInput" placeholder="Admin Username" style="width:100%; padding:10px; margin-bottom:12px; background:var(--bg-card); border:1px solid var(--border-color); border-radius:6px; color:var(--text); box-sizing:border-box; outline:none;" />
        <input type="password" id="authAdminPasswordInput" placeholder="Admin Password" style="width:100%; padding:10px; margin-bottom:15px; background:var(--bg-card); border:1px solid var(--border-color); border-radius:6px; color:var(--text); box-sizing:border-box; outline:none;" />
        
        <label style="display:flex; align-items:flex-start; gap:8px; text-align:left; font-size:0.8rem; color:var(--text-main); margin-bottom:20px; cursor:pointer;">
            <input type="checkbox" id="authConfirmCheckbox" style="margin-top:3px;" />
            <span>I confirm and authorize the permanent deletion of all application data.</span>
        </label>

        <div style="display:flex; gap:12px; justify-content:center;">
            <button class="btn-clear" id="resetAuthCancelBtn" style="flex:1; height:40px; border:1px solid var(--border-color); color:var(--text-main); border-radius:8px; font-weight:600; cursor:pointer;">Cancel</button>
            <button class="btn-primary" id="resetAuthVerifyBtn" style="flex:1.5; height:40px; background:#ef4444 !important; color:white; border:none; border-radius:8px; font-weight:600; cursor:pointer;">Authorize Reset</button>
        </div>
    `;

    overlay.appendChild(card);
    document.body.appendChild(overlay);

    const closePrompt = () => {
        document.body.removeChild(overlay);
    };

    overlay.querySelector('#resetAuthCancelBtn').onclick = closePrompt;

    overlay.querySelector('#resetAuthVerifyBtn').onclick = () => {
        const username = card.querySelector('#authAdminUsernameInput').value.trim();
        const password = card.querySelector('#authAdminPasswordInput').value.trim();
        const checkbox = card.querySelector('#authConfirmCheckbox').checked;

        if (!username || !password) {
            showToast("Admin credentials cannot be empty", "error");
            return;
        }
        if (!checkbox) {
            showToast("Please check the confirmation box to authorize", "error");
            return;
        }

        closePrompt();
        showFinalConfirmation(username, password);
    };
}

function showFinalConfirmation(username, password) {
    showDeleteConfirm(
        "FINAL CONFIRMATION",
        "All inventory, recipes, sales records, stock data, and other application data will be permanently deleted.\n\nThis cannot be undone.",
        async () => {
            await executeFactoryReset(username, password);
        },
        "YES, RESET ALL DATA"
    );
}

async function executeFactoryReset(username, password) {
    const loadingOverlay = document.createElement('div');
    loadingOverlay.className = 'overlay';
    loadingOverlay.style.zIndex = '999999';
    loadingOverlay.style.display = 'flex';
    loadingOverlay.style.alignItems = 'center';
    loadingOverlay.style.justifyContent = 'center';
    loadingOverlay.style.background = 'rgba(0, 0, 0, 0.6)';
    loadingOverlay.style.backdropFilter = 'blur(5px)';

    const card = document.createElement('div');
    card.className = 'scanner-card';
    card.style.maxWidth = '320px';
    card.style.padding = '30px';
    card.style.textAlign = 'center';
    card.innerHTML = `
        <div class="spinner" style="margin: 0 auto 20px auto; width: 40px; height: 40px; border: 4px solid rgba(255,255,255,0.1); border-top-color: var(--accent); border-radius: 50%; animation: spin 1s linear infinite;"></div>
        <div style="color:white; font-weight:bold; font-size:1.05rem;" id="resetLoadingText">Resetting application data...</div>
    `;
    
    if (!document.getElementById('spinnerAnimStyle')) {
        const style = document.createElement('style');
        style.id = 'spinnerAnimStyle';
        style.innerHTML = "@keyframes spin { 0% { transform: rotate(0deg); } 100% { transform: rotate(360deg); } }";
        document.head.appendChild(style);
    }

    loadingOverlay.appendChild(card);
    document.body.appendChild(loadingOverlay);

    try {
        const res = await fetch(`${API_BASE}/api/factory-reset`, {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify({ adminUsername: username, adminPassword: password })
        });

        if (res.ok) {
            const data = await res.json();
            if (data.success) {
                document.getElementById('resetLoadingText').innerText = "Factory Reset Complete";
                card.querySelector('.spinner').style.display = 'none';
                
                const okBtn = document.createElement('button');
                okBtn.className = 'btn-primary';
                okBtn.innerText = 'OK';
                okBtn.style.marginTop = '20px';
                okBtn.style.width = '100%';
                okBtn.onclick = () => {
                    document.body.removeChild(loadingOverlay);
                    sessionStorage.clear();
                    localStorage.clear();
                    location.reload();
                };
                card.appendChild(okBtn);
            } else {
                document.body.removeChild(loadingOverlay);
                showToast(data.message || "Failed to perform factory reset", "error");
            }
        } else {
            document.body.removeChild(loadingOverlay);
            if (res.status === 401) {
                showToast("Admin authorization failed. Invalid password.", "error");
            } else {
                showToast("Server error during factory reset", "error");
            }
        }
    } catch (e) {
        document.body.removeChild(loadingOverlay);
        showToast("Connection error during factory reset", "error");
    }
}

window.showLowStockDetails = function() {
    const list = document.getElementById('stockDetailsList');
    if (!list) return;
    
    // Filter out inactive products if status exists
    const lowStockItems = allProducts.filter(p => {
        const stockVal = parseFloat(p.stock || 0);
        const minVal = parseFloat(p.minStock || 0);
        return stockVal <= minVal && stockVal > 0;
    });
    
    document.getElementById('stockDetailsTitle').innerText = `Low Stock Items (${lowStockItems.length})`;
    
    list.innerHTML = '';
    if (lowStockItems.length === 0) {
        list.innerHTML = '<tr><td colspan="3" style="padding:15px; text-align:center; color:rgba(255,255,255,0.4);">No low stock items</td></tr>';
    } else {
        lowStockItems.forEach(p => {
            const tr = document.createElement('tr');
            tr.style.borderBottom = '1px solid rgba(255,255,255,0.05)';
            const uom = p.bigUnit || 'pcs';
            tr.innerHTML = `
                <td style="padding:10px; text-align:left; font-weight:600; color:white;">${escapeHtml(p.name)}</td>
                <td style="padding:10px; text-align:right; color:var(--warn); font-weight:bold;">${parseFloat(p.stock).toFixed(2)} ${uom}</td>
                <td style="padding:10px; text-align:right; color:rgba(255,255,255,0.6);">${parseFloat(p.minStock).toFixed(2)} ${uom}</td>
            `;
            list.appendChild(tr);
        });
    }
    
    document.getElementById('stockDetailsModal').classList.remove('hidden');
};

window.showOutOfStockDetails = function() {
    const list = document.getElementById('stockDetailsList');
    if (!list) return;
    
    const outOfStockItems = allProducts.filter(p => parseFloat(p.stock || 0) <= 0);
    
    document.getElementById('stockDetailsTitle').innerText = `Out of Stock Items (${outOfStockItems.length})`;
    
    list.innerHTML = '';
    if (outOfStockItems.length === 0) {
        list.innerHTML = '<tr><td colspan="3" style="padding:15px; text-align:center; color:rgba(255,255,255,0.4);">No out of stock items</td></tr>';
    } else {
        outOfStockItems.forEach(p => {
            const tr = document.createElement('tr');
            tr.style.borderBottom = '1px solid rgba(255,255,255,0.05)';
            const uom = p.bigUnit || 'pcs';
            tr.innerHTML = `
                <td style="padding:10px; text-align:left; font-weight:600; color:white;">${escapeHtml(p.name)}</td>
                <td style="padding:10px; text-align:right; color:var(--danger); font-weight:bold;">${parseFloat(p.stock).toFixed(2)} ${uom}</td>
                <td style="padding:10px; text-align:right; color:rgba(255,255,255,0.6);">${parseFloat(p.minStock).toFixed(2)} ${uom}</td>
            `;
            list.appendChild(tr);
        });
    }
    
    document.getElementById('stockDetailsModal').classList.remove('hidden');
};

window.closeStockDetailsModal = function() {
    document.getElementById('stockDetailsModal').classList.add('hidden');
};

// ─── PRODUCTION TAB FUNCTIONALITY ────────────────────────────────────────

window.loadProductionTab = function() {
    console.log("Loading Production Tab...");
    
    // 1. Populate Material Select
    const matSelect = document.getElementById('prodMaterialSelect');
    if (!matSelect) return;
    matSelect.innerHTML = '<option value="">-- Choose Material --</option>';
    
    // Only active items with stock or tracked
    const materials = allProducts.filter(p => p.status !== 'Inactive');
    materials.sort((a, b) => a.name.localeCompare(b.name));
    
    materials.forEach(p => {
        const opt = document.createElement('option');
        opt.value = p.id;
        opt.textContent = p.name;
        matSelect.appendChild(opt);
    });
    
    // Reset inputs and outputs
    document.getElementById('prodMaterialStockDisplay').style.display = 'none';
    document.getElementById('prodRecipeDetailsDisplay').style.display = 'none';
    
    const recipeSelect = document.getElementById('prodRecipeSelect');
    recipeSelect.innerHTML = '<option value="">-- Choose Recipe (Select Material First) --</option>';
    recipeSelect.disabled = true;
    
    const materialUnitSelect = document.getElementById('prodMaterialUnitSelect');
    materialUnitSelect.innerHTML = '';
    
    // Populate base/sub units dropdowns with standard system units
    const baseUnitSelect = document.getElementById('prodBaseUnitSelect');
    const subUnitSelect = document.getElementById('prodSubUnitSelect');
    
    const standardUnits = [
        { val: "gallon", text: "Gallon" },
        { val: "kg", text: "Kilogram (kg)" },
        { val: "l", text: "Liter (l)" },
        { val: "box", text: "Box" },
        { val: "bottle", text: "Bottle" },
        { val: "cup", text: "Cup" },
        { val: "g", text: "Gram (g)" },
        { val: "ml", text: "Milliliter (ml)" },
        { val: "pcs", text: "Piece (pcs)" }
    ];
    
    baseUnitSelect.innerHTML = '<option value="">-- Select Base Unit --</option>';
    subUnitSelect.innerHTML = '<option value="">-- Select Sub Unit --</option>';
    
    standardUnits.forEach(u => {
        const opt1 = document.createElement('option');
        opt1.value = u.val;
        opt1.textContent = u.text;
        baseUnitSelect.appendChild(opt1);
        
        const opt2 = document.createElement('option');
        opt2.value = u.val;
        opt2.textContent = u.text;
        subUnitSelect.appendChild(opt2);
    });
    
    // Set some defaults
    baseUnitSelect.value = "gallon";
    subUnitSelect.value = "cup";
    
    // Reset displays
    document.getElementById('prodEstBaseDisplay').textContent = '-';
    document.getElementById('prodEstSubDisplay').textContent = '-';
    document.getElementById('prodDeviationDisplay').textContent = '-';
    
    const valBox = document.getElementById('prodValidationBox');
    valBox.classList.add('hidden');
    valBox.textContent = '';
};

window.onProductionMaterialChange = function() {
    const matSelect = document.getElementById('prodMaterialSelect');
    const materialId = parseInt(matSelect.value);
    
    const stockDisplay = document.getElementById('prodMaterialStockDisplay');
    const stockQtySpan = document.getElementById('prodMaterialStockQty');
    const stockUnitSpan = document.getElementById('prodMaterialStockUnit');
    const recipeSelect = document.getElementById('prodRecipeSelect');
    const materialUnitSelect = document.getElementById('prodMaterialUnitSelect');
    
    // Clear outputs
    document.getElementById('prodEstBaseDisplay').textContent = '-';
    document.getElementById('prodEstSubDisplay').textContent = '-';
    document.getElementById('prodDeviationDisplay').textContent = '-';
    
    if (isNaN(materialId)) {
        stockDisplay.style.display = 'none';
        recipeSelect.innerHTML = '<option value="">-- Choose Recipe (Select Material First) --</option>';
        recipeSelect.disabled = true;
        materialUnitSelect.innerHTML = '';
        return;
    }
    
    const product = allProducts.find(p => p.id === materialId);
    if (!product) return;
    
    // Show available stock
    const primaryUnit = product.bigUnit || product.unitOfMeasure || 'pcs';
    stockQtySpan.textContent = formatDynamicNumber(product.stock || 0);
    stockUnitSpan.textContent = primaryUnit;
    stockDisplay.style.display = 'block';
    
    // Populate material unit options (e.g. big/small units)
    materialUnitSelect.innerHTML = '';
    if (product.bigUnit && product.smallUnit) {
        const optBig = document.createElement('option');
        optBig.value = product.bigUnit.toLowerCase();
        optBig.textContent = product.bigUnit;
        materialUnitSelect.appendChild(optBig);
        
        const optSmall = document.createElement('option');
        optSmall.value = product.smallUnit.toLowerCase();
        optSmall.textContent = product.smallUnit;
        materialUnitSelect.appendChild(optSmall);
    } else {
        const opt = document.createElement('option');
        const u = (product.unitOfMeasure || 'pcs').toLowerCase();
        opt.value = u;
        opt.textContent = product.unitOfMeasure || 'pcs';
        materialUnitSelect.appendChild(opt);
    }
    
    // Auto-select Base and Sub units based on the selected material's units in inventory
    const baseUnitSelect = document.getElementById('prodBaseUnitSelect');
    const subUnitSelect = document.getElementById('prodSubUnitSelect');

    const materialBaseUnit = product.bigUnit || product.unitOfMeasure || 'pcs';
    const materialSubUnit = product.smallUnit || product.unitOfMeasure || 'pcs';

    // Helper function to find or add and select unit in select dropdown
    const selectUnitInDropdown = (selectEl, unitText) => {
        if (!unitText) return;
        const targetVal = unitText.toLowerCase().trim();
        let found = false;
        
        // Search options
        for (let i = 0; i < selectEl.options.length; i++) {
            if (selectEl.options[i].value === targetVal) {
                selectEl.value = targetVal;
                found = true;
                break;
            }
        }
        
        // If not found, add it
        if (!found) {
            const opt = document.createElement('option');
            opt.value = targetVal;
            opt.textContent = unitText;
            selectEl.appendChild(opt);
            selectEl.value = targetVal;
        }
    };

    selectUnitInDropdown(baseUnitSelect, materialBaseUnit);
    selectUnitInDropdown(subUnitSelect, materialSubUnit);

    // Find recipes that use this material
    const matchingRecipes = allRecipes.filter(r => 
        r.parts && r.parts.some(p => Number(p.partId) === Number(materialId))
    );
    
    if (matchingRecipes.length === 0) {
        recipeSelect.innerHTML = '<option value="">No recipes found using this material</option>';
        recipeSelect.disabled = true;
    } else {
        recipeSelect.disabled = false;
        recipeSelect.innerHTML = '<option value="">-- Choose Recipe --</option>';
        matchingRecipes.forEach(r => {
            const opt = document.createElement('option');
            opt.value = r.id;
            opt.textContent = r.name;
            recipeSelect.appendChild(opt);
        });
    }
    
    document.getElementById('prodRecipeDetailsDisplay').style.display = 'none';
    
    // Try to auto-select recipe if there is only one
    if (matchingRecipes.length === 1) {
        recipeSelect.value = matchingRecipes[0].id;
        window.onProductionRecipeChange();
    }
};

window.onProductionRecipeChange = function() {
    const recipeSelect = document.getElementById('prodRecipeSelect');
    const recipeId = parseInt(recipeSelect.value);
    
    const recipeDetailsDisplay = document.getElementById('prodRecipeDetailsDisplay');
    const recipeYieldSpan = document.getElementById('prodRecipeYieldDisplay');
    
    // Clear outputs
    document.getElementById('prodEstBaseDisplay').textContent = '-';
    document.getElementById('prodEstSubDisplay').textContent = '-';
    document.getElementById('prodDeviationDisplay').textContent = '-';
    
    if (isNaN(recipeId)) {
        recipeDetailsDisplay.style.display = 'none';
        return;
    }
    
    const recipe = allRecipes.find(r => r.id === recipeId);
    if (!recipe) return;
    
    const yieldQty = recipe.yieldQuantity || 1;
    const yieldUnit = recipe.yieldUnit || 'pcs';
    recipeYieldSpan.textContent = `${yieldQty} ${yieldUnit}`;
    recipeDetailsDisplay.style.display = 'block';
    
    // Base and sub units are kept as defaulted from the selected material (ingredient)
    window.triggerProductionCalculation();
};

window.triggerProductionCalculation = function() {
    window.calculateProduction();
};

window.calculateProduction = function() {
    const valBox = document.getElementById('prodValidationBox');
    valBox.classList.add('hidden');
    valBox.textContent = '';
    
    const matSelect = document.getElementById('prodMaterialSelect');
    const recipeSelect = document.getElementById('prodRecipeSelect');
    const matQtyInput = document.getElementById('prodMaterialQty');
    const matUnitSelect = document.getElementById('prodMaterialUnitSelect');
    const baseUnitSelect = document.getElementById('prodBaseUnitSelect');
    const subUnitSelect = document.getElementById('prodSubUnitSelect');
    
    const matId = parseInt(matSelect.value);
    const recId = parseInt(recipeSelect.value);
    const matQty = parseFloat(matQtyInput.value);
    const matUnit = matUnitSelect.value;
    const baseUnit = baseUnitSelect.value;
    const subUnit = subUnitSelect.value;
    
    // Reset output text
    const baseDisplay = document.getElementById('prodEstBaseDisplay');
    const subDisplay = document.getElementById('prodEstSubDisplay');
    const devDisplay = document.getElementById('prodDeviationDisplay');
    
    baseDisplay.textContent = '-';
    subDisplay.textContent = '-';
    devDisplay.textContent = '-';
    
    // 12. Validation
    if (isNaN(matId)) {
        showValidation("Please select an Inventory Material.");
        return;
    }
    if (isNaN(recId)) {
        showValidation("Please select a Recipe.");
        return;
    }
    if (isNaN(matQty) || matQty <= 0) {
        showValidation("Material quantity must be greater than zero.");
        return;
    }
    if (!matUnit) {
        showValidation("Please select the material unit.");
        return;
    }
    if (!baseUnit) {
        showValidation("Please select the output Base Unit.");
        return;
    }
    if (!subUnit) {
        showValidation("Please select the output Sub Unit.");
        return;
    }
    
    const product = allProducts.find(p => p.id === matId);
    const recipe = allRecipes.find(r => r.id === recId);
    
    if (!product || !recipe) {
        showValidation("Required inventory/recipe data was not found.");
        return;
    }
    
    // Find the ingredient required by the recipe
    const ingredient = recipe.parts.find(p => Number(p.partId) === Number(matId));
    if (!ingredient) {
        showValidation(`Selected recipe does not use ${product.name} as an ingredient.`);
        return;
    }
    
    const reqQty = parseFloat(ingredient.qty);
    const reqUnit = ingredient.unitOfMeasure || 'pcs';
    
    if (reqQty <= 0) {
        showValidation("Required recipe ingredient quantity is invalid.");
        return;
    }
    
    // Convert entered material quantity to the recipe ingredient required unit
    const convertedInputQty = convertUnits(matQty, matUnit, reqUnit, product);
    
    // Calculate recipe-based output in the recipe's yield unit
    const yieldQty = parseFloat(recipe.yieldQuantity) || 1.0;
    const yieldUnit = recipe.yieldUnit || 'pcs';
    
    // Formula: Production Quantity = Available Material Quantity ÷ Material Required Per Recipe × Recipe Output Quantity
    const recipeProductionOutput = (convertedInputQty / reqQty) * yieldQty;
    
    // Convert actual production output to selected Sub Unit
    const actualSubUnitProduction = convertUnits(recipeProductionOutput, yieldUnit, subUnit);
    
    // Convert actual production output to selected Base Unit
    const actualBaseUnitProduction = convertUnits(actualSubUnitProduction, subUnit, baseUnit);
    
    // Display actual production yield
    const expectedBaseUnitProduction = convertUnits(matQty, matUnit, baseUnit, product);
    const conversionFactor = convertUnits(1.0, baseUnit, subUnit);
    const expectedSubUnitProduction = expectedBaseUnitProduction * conversionFactor;
    
    // Deviation = Expected Sub Unit Production - Actual Sub Unit Production
    const deviation = expectedSubUnitProduction - actualSubUnitProduction;
    
    // Render outputs nicely
    baseDisplay.innerHTML = `${formatDynamicNumber(expectedBaseUnitProduction)} <span style="font-size:0.8rem; font-weight:normal;">${baseUnit}</span>`;
    subDisplay.innerHTML = `${formatDynamicNumber(actualSubUnitProduction)} <span style="font-size:0.8rem; font-weight:normal;">${subUnit}</span>`;
    
    if (Math.abs(deviation) < 0.001) {
        devDisplay.textContent = "No Deviation";
        devDisplay.style.color = "#10b981"; // green
    } else {
        devDisplay.innerHTML = `${formatDynamicNumber(deviation)} <span style="font-size:0.8rem; font-weight:normal;">${subUnit}</span>`;
        devDisplay.style.color = "var(--danger)"; // red
    }
    
    function showValidation(msg) {
        valBox.textContent = msg;
        valBox.classList.remove('hidden');
    }
};

// Robust helper function for unit conversion
function convertUnits(qty, fromUnit, toUnit, product = null) {
    if (!fromUnit || !toUnit) return qty;
    fromUnit = fromUnit.toLowerCase().trim();
    toUnit = toUnit.toLowerCase().trim();
    if (fromUnit === toUnit) return qty;

    // 1. If it's a specific product (like the selected material) and the units match its big/small units
    if (product && product.bigUnit && product.smallUnit) {
        const big = product.bigUnit.toLowerCase().trim();
        const small = product.smallUnit.toLowerCase().trim();
        const conv = parseFloat(product.conversionValue) || 1.0;
        if (fromUnit === big && toUnit === small) {
            return qty * conv;
        }
        if (fromUnit === small && toUnit === big) {
            return qty / conv;
        }
    }

    // 2. Also check if there is ANY product in inventory that has these big/small units and use its conversion factor
    if (allProducts && allProducts.length > 0) {
        const matchingProd = allProducts.find(p => 
            p.bigUnit && p.smallUnit && 
            p.bigUnit.toLowerCase().trim() === fromUnit && 
            p.smallUnit.toLowerCase().trim() === toUnit
        );
        if (matchingProd) {
            return qty * (parseFloat(matchingProd.conversionValue) || 1.0);
        }
        const matchingProdRev = allProducts.find(p => 
            p.bigUnit && p.smallUnit && 
            p.smallUnit.toLowerCase().trim() === fromUnit && 
            p.bigUnit.toLowerCase().trim() === toUnit
        );
        if (matchingProdRev) {
            return qty / (parseFloat(matchingProdRev.conversionValue) || 1.0);
        }
    }

    // 3. Fallback to standard conversions
    // Weight
    if ((fromUnit === "kg" || fromUnit === "kilogram" || fromUnit === "kilograms") && (toUnit === "g" || toUnit === "gram" || toUnit === "grams")) return qty * 1000.0;
    if ((fromUnit === "g" || fromUnit === "gram" || fromUnit === "grams") && (toUnit === "kg" || toUnit === "kilogram" || toUnit === "kilograms")) return qty / 1000.0;
    // Volume
    if ((fromUnit === "l" || fromUnit === "liter" || fromUnit === "liters") && (toUnit === "ml" || toUnit === "milliliter" || toUnit === "milliliters")) return qty * 1000.0;
    if ((fromUnit === "ml" || fromUnit === "milliliter" || fromUnit === "milliliters") && (toUnit === "l" || toUnit === "liter" || toUnit === "liters")) return qty / 1000.0;
    // Gallon to Cup (from the example: 1 Gallon = 16 Cups)
    if ((fromUnit === "gallon" || fromUnit === "gal" || fromUnit === "gallons") && (toUnit === "cup" || toUnit === "cups")) return qty * 16.0;
    if ((fromUnit === "cup" || fromUnit === "cups") && (toUnit === "gallon" || toUnit === "gal" || toUnit === "gallons")) return qty / 16.0;

    return qty; // If no conversion rule matches, return original qty
}


