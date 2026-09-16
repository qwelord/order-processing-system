const API_HOST = window.location.hostname || 'localhost';
const API = `http://${API_HOST}:5001`;
const NOTIFY = `http://${API_HOST}:5003`;

const state = {
    products: [],
    orders: [],
    cart: [],
    paymentOrderId: null,
    orderPoll: null
};

const pageMeta = {
    dashboard: ['Overview', 'A live view of your order processing flow.'],
    catalog: ['Product catalog', 'Manage products and inventory.'],
    orders: ['Orders', 'Search, inspect, pay and cancel pending orders.']
};

const money = value => new Intl.NumberFormat('en-DE', {
    style: 'currency',
    currency: 'EUR'
}).format(value || 0);

const escapeHtml = value => String(value ?? '').replace(/[&<>"']/g, character => ({
    '&': '&amp;',
    '<': '&lt;',
    '>': '&gt;',
    '"': '&quot;',
    "'": '&#39;'
}[character]));

const statusClass = status => String(status || '').toLowerCase().replace('pendingpayment', 'pending');
const formatDate = value => new Date(value).toLocaleString();

function toast(message, type = 'success') {
    const element = document.createElement('div');
    element.className = `toast ${type}`;
    element.textContent = message;
    document.getElementById('toasts').appendChild(element);
    setTimeout(() => element.remove(), 4200);
}

function openModal(id) {
    const modal = document.getElementById(id);
    modal.classList.add('open');
    modal.setAttribute('aria-hidden', 'false');
}

function closeModal(id) {
    const modal = document.getElementById(id);
    modal.classList.remove('open');
    modal.setAttribute('aria-hidden', 'true');
}

function showPage(page) {
    document.querySelectorAll('.page').forEach(element => element.classList.add('hidden'));
    document.getElementById(`page-${page}`).classList.remove('hidden');
    document.querySelectorAll('.nav-button').forEach(button => {
        button.classList.toggle('active', button.dataset.page === page);
    });

    const [title, subtitle] = pageMeta[page];
    document.getElementById('pageTitle').textContent = title;
    document.getElementById('pageSubtitle').textContent = subtitle;

    if (page === 'catalog') loadProducts();
    if (page === 'orders') loadOrders();
}

document.querySelectorAll('.nav-button').forEach(button => {
    button.addEventListener('click', () => showPage(button.dataset.page));
});

document.querySelectorAll('.modal').forEach(modal => {
    modal.addEventListener('click', event => {
        if (event.target === modal)
            closeModal(modal.id);
    });
});

document.addEventListener('keydown', event => {
    if (event.key !== 'Escape') return;
    document.querySelectorAll('.modal.open').forEach(modal => closeModal(modal.id));
});

async function api(path, options = {}) {
    const response = await fetch(`${API}${path}`, {
        headers: {
            'Content-Type': 'application/json',
            ...(options.headers || {})
        },
        ...options
    });

    if (response.ok)
        return response.status === 204 ? null : response.json();

    let message = 'Request failed';

    try {
        const payload = await response.json();
        if (payload.errors)
            message = Object.values(payload.errors).flat().join(' ');
        else
            message = payload.detail || payload.message || payload.title || message;
    } catch {
        message = response.statusText || message;
    }

    throw new Error(message);
}

async function refreshAll() {
    await Promise.all([loadProducts(), loadOrders(), loadDashboard()]);
}

async function loadProducts() {
    try {
        state.products = await api('/api/products');
        renderProducts();
    } catch (error) {
        toast(error.message, 'error');
    }
}

function renderProducts() {
    const query = (document.getElementById('productSearch')?.value || '').toLowerCase();
    const products = state.products.filter(product =>
        `${product.name} ${product.description}`.toLowerCase().includes(query));
    const root = document.getElementById('productGrid');

    if (!products.length) {
        root.innerHTML = '<div class="empty">No products match your search.</div>';
        return;
    }

    root.innerHTML = products.map(product => `
        <article class="product-card">
            <div class="row-actions">
                <span class="badge ${product.stockQuantity > 0 ? 'info' : 'cancelled'}">${product.stockQuantity} in stock</span>
                ${product.isActive ? '<span class="badge paid">Active</span>' : '<span class="badge cancelled">Archived</span>'}
            </div>
            <div class="product-title product-title-spaced">${escapeHtml(product.name)}</div>
            <div class="product-description">${escapeHtml(product.description || 'No description')}</div>
            <div class="product-meta">
                <div class="product-price">${money(product.price)}</div>
                <div class="product-stock">Inventory · ${product.stockQuantity}</div>
            </div>
            <div class="row-actions product-actions">
                <button class="button button-primary product-add-button" ${!product.isActive || product.stockQuantity < 1 ? 'disabled' : ''} onclick="addToCart('${product.id}')" type="button">Add to cart</button>
                <button class="button button-ghost" onclick="openProductModal('${product.id}')" type="button">Edit</button>
                <button class="button button-danger" onclick="archiveProduct('${product.id}')" type="button" aria-label="Archive product">×</button>
            </div>
        </article>
    `).join('');
}

function openProductModal(id) {
    const product = state.products.find(item => item.id === id);
    document.getElementById('productModalTitle').textContent = product ? 'Edit product' : 'New product';
    document.getElementById('productId').value = product?.id || '';
    document.getElementById('productName').value = product?.name || '';
    document.getElementById('productDescription').value = product?.description || '';
    document.getElementById('productPrice').value = product?.price || '';
    document.getElementById('productStock').value = product?.stockQuantity ?? 0;
    document.getElementById('productActive').checked = product?.isActive ?? true;
    openModal('productModal');
}

async function saveProduct(event) {
    event.preventDefault();

    const id = document.getElementById('productId').value;
    const body = {
        name: document.getElementById('productName').value,
        description: document.getElementById('productDescription').value,
        price: Number(document.getElementById('productPrice').value),
        stockQuantity: Number(document.getElementById('productStock').value),
        isActive: document.getElementById('productActive').checked
    };

    try {
        await api(id ? `/api/products/${id}` : '/api/products', {
            method: id ? 'PUT' : 'POST',
            body: JSON.stringify(body)
        });

        closeModal('productModal');
        await loadProducts();
        toast(id ? 'Product updated' : 'Product created');
    } catch (error) {
        toast(error.message, 'error');
    }
}

async function archiveProduct(id) {
    if (!confirm('Archive this product?')) return;

    try {
        await api(`/api/products/${id}`, { method: 'DELETE' });
        await loadProducts();
        toast('Product archived');
    } catch (error) {
        toast(error.message, 'error');
    }
}

function addToCart(productId) {
    const product = state.products.find(item => item.id === productId);
    if (!product) return;

    const item = state.cart.find(entry => entry.productId === productId);
    if (item) {
        if (item.quantity >= product.stockQuantity) {
            toast('Not enough stock', 'error');
            return;
        }
        item.quantity += 1;
    } else {
        state.cart.push({ productId, quantity: 1 });
    }

    renderCart();
    openModal('orderModal');
}

function changeQuantity(productId, delta) {
    const item = state.cart.find(entry => entry.productId === productId);
    if (!item) return;

    const product = state.products.find(entry => entry.id === productId);
    item.quantity += delta;

    if (item.quantity <= 0) {
        state.cart = state.cart.filter(entry => entry !== item);
    } else if (item.quantity > (product?.stockQuantity || 0)) {
        item.quantity = product.stockQuantity;
    }

    renderCart();
}

function renderCart() {
    const root = document.getElementById('cartItems');

    if (!state.cart.length) {
        root.innerHTML = '<div class="empty">Your cart is empty. Add products from the catalog.</div>';
        document.getElementById('cartTotal').textContent = '€0.00';
        return;
    }

    let total = 0;
    root.innerHTML = state.cart.map(item => {
        const product = state.products.find(entry => entry.id === item.productId);
        if (!product) return '';

        const lineTotal = product.price * item.quantity;
        total += lineTotal;

        return `
            <div class="cart-item">
                <div>
                    <strong>${escapeHtml(product.name)}</strong>
                    <div class="muted">${money(product.price)} each</div>
                </div>
                <div class="qty">
                    <button type="button" onclick="changeQuantity('${product.id}', -1)">−</button>
                    <strong>${item.quantity}</strong>
                    <button type="button" onclick="changeQuantity('${product.id}', 1)">+</button>
                </div>
                <strong>${money(lineTotal)}</strong>
            </div>
        `;
    }).join('');

    document.getElementById('cartTotal').textContent = money(total);
}

function openOrderModal() {
    renderCart();
    openModal('orderModal');
}

async function createOrder(event) {
    event.preventDefault();

    if (!state.cart.length) {
        toast('Add at least one product', 'error');
        return;
    }

    const body = {
        customerName: document.getElementById('customerName').value,
        customerEmail: document.getElementById('customerEmail').value,
        items: state.cart.map(item => ({ productId: item.productId, quantity: item.quantity })),
        paymentMethod: document.getElementById('paymentMethod').value
    };

    try {
        const order = await api('/api/orders', {
            method: 'POST',
            body: JSON.stringify(body)
        });

        state.cart = [];
        closeModal('orderModal');
        await refreshAll();

        if (order.paymentMethod === 'Card') {
            openPaymentModal(order);
            return;
        }

        toast(`Order #${order.id.slice(0, 8)} created`);
        openOrderDetails(order.id);
    } catch (error) {
        toast(error.message, 'error');
    }
}

async function openPaymentModalById(orderId) {
    try {
        const order = await api(`/api/orders/${orderId}`);
        openPaymentModal(order);
    } catch (error) {
        toast(error.message, 'error');
    }
}

function openPaymentModal(order) {
    state.paymentOrderId = order.id;
    document.getElementById('paymentOrderShort').textContent = `#${order.id.slice(0, 8)}`;
    document.getElementById('paymentAmount').textContent = money(order.totalAmount);
    document.getElementById('paymentOrderStatus').textContent = order.status === 'PendingPayment' ? 'Pending payment' : order.status;
    document.getElementById('paymentOrderStatus').className = `badge ${statusClass(order.status)}`;
    document.getElementById('paymentCardNumber').value = '';
    document.getElementById('paymentExpiry').value = '';
    document.getElementById('paymentCvv').value = '';
    openModal('paymentModal');
}

function normalizeCardNumber(value) {
    return value.replace(/\D/g, '').slice(0, 16);
}

async function payOrder(event) {
    event.preventDefault();
    const button = document.getElementById('payButton');
    const cardDigits = normalizeCardNumber(document.getElementById('paymentCardNumber').value);

    if (cardDigits.length < 4) {
        toast('Enter a valid demo card number', 'error');
        return;
    }

    button.disabled = true;
    button.textContent = 'Processing payment…';

    try {
        const order = await api(`/api/orders/${state.paymentOrderId}/pay`, {
            method: 'POST',
            body: JSON.stringify({ cardLast4: cardDigits.slice(-4) })
        });

        closeModal('paymentModal');
        await refreshAll();
        await openOrderDetails(order.id);

        if (order.status === 'Paid')
            toast(`Payment approved · Order #${order.id.slice(0, 8)}`);
        else
            toast('Payment was declined. Reserved stock was released.', 'error');
    } catch (error) {
        toast(error.message, 'error');
    } finally {
        button.disabled = false;
        button.textContent = 'Pay now';
    }
}

async function loadOrders() {
    try {
        const search = encodeURIComponent(document.getElementById('orderSearch')?.value || '');
        const status = encodeURIComponent(document.getElementById('orderStatus')?.value || '');
        state.orders = await api(`/api/orders?search=${search}&status=${status}`);
        renderOrders();
    } catch (error) {
        toast(error.message, 'error');
    }
}

function renderOrders() {
    const root = document.getElementById('ordersTable');

    if (!state.orders.length) {
        root.innerHTML = '<div class="empty">No orders found.</div>';
        return;
    }

    root.innerHTML = `
        <table class="table">
            <thead>
                <tr>
                    <th>Order</th><th>Customer</th><th>Items</th><th>Total</th><th>Status</th><th>Payment</th><th>Created</th><th></th>
                </tr>
            </thead>
            <tbody>
                ${state.orders.map(order => `
                    <tr>
                        <td><strong>#${order.id.slice(0, 8)}</strong></td>
                        <td>${escapeHtml(order.customerName)}</td>
                        <td>${order.itemCount}</td>
                        <td>${money(order.totalAmount)}</td>
                        <td><span class="badge ${statusClass(order.status)}">${escapeHtml(order.status === 'PendingPayment' ? 'Pending payment' : order.status)}</span></td>
                        <td>${escapeHtml(order.paymentMethod || '—')}</td>
                        <td>${formatDate(order.createdAt)}</td>
                        <td><button class="button button-ghost" onclick="openOrderDetails('${order.id}')" type="button">View</button></td>
                    </tr>
                `).join('')}
            </tbody>
        </table>
    `;
}

async function openOrderDetails(id) {
    try {
        const order = await api(`/api/orders/${id}`);
        document.getElementById('orderDetails').innerHTML = orderDetailsHtml(order);
        openModal('detailsModal');
        startOrderPolling(order);
    } catch (error) {
        toast(error.message, 'error');
    }
}

function orderDetailsHtml(order) {
    const canPay = order.status === 'PendingPayment' && order.paymentMethod === 'Card';
    const canCancel = order.status === 'PendingPayment';

    return `
        <div class="detail-grid">
            <div class="detail-card">
                <div class="dialog-header">
                    <div>
                        <div class="section-kicker">ORDER</div>
                        <h3>#${order.id.slice(0, 8)}</h3>
                    </div>
                    <span class="badge ${statusClass(order.status)}">${escapeHtml(order.status === 'PendingPayment' ? 'Pending payment' : order.status)}</span>
                </div>
                <div class="detail-row"><span class="detail-muted">Customer</span><span>${escapeHtml(order.customerName)}</span></div>
                <div class="detail-row"><span class="detail-muted">Email</span><span>${escapeHtml(order.customerEmail)}</span></div>
                <div class="detail-row"><span class="detail-muted">Payment</span><span>${escapeHtml(order.paymentMethod)}</span></div>
                <div class="detail-row"><span class="detail-muted">Created</span><span>${formatDate(order.createdAt)}</span></div>
                ${order.items.map(item => `
                    <div class="detail-row"><span>${escapeHtml(item.productName)} × ${item.quantity}</span><strong>${money(item.lineTotal)}</strong></div>
                `).join('')}
                <div class="detail-row"><strong>Total</strong><strong>${money(order.totalAmount)}</strong></div>
                <div class="row-actions detail-actions">
                    ${canPay ? `<button class="button button-primary" onclick="openPaymentModalById('${order.id}')" type="button">Pay now</button>` : ''}
                    ${canCancel ? `<button class="button button-danger" onclick="cancelOrder('${order.id}')" type="button">Cancel order</button>` : ''}
                </div>
            </div>
            <div class="detail-card">
                <div class="section-kicker">FLOW</div>
                <div class="timeline">
                    ${flowStep('Order created', 'Inventory reserved', order.status !== 'Cancelled')}
                    ${flowStep('Payment', order.paymentMethod === 'Card' ? 'Waiting for card payment' : 'Cash on delivery', order.status === 'Paid' || order.paymentMethod === 'CashOnDelivery')}
                    ${flowStep('Completed', order.status === 'Paid' ? 'Payment confirmed by Payment Service' : order.status === 'Cancelled' ? 'Payment declined or order cancelled' : 'Awaiting payment', order.status === 'Paid' || order.status === 'Cancelled')}
                </div>
            </div>
        </div>
    `;
}

function flowStep(title, description, done) {
    return `
        <div class="step ${done ? 'done' : ''}">
            <div class="step-marker">✓</div>
            <div>
                <div class="step-title">${escapeHtml(title)}</div>
                <div class="step-description">${escapeHtml(description)}</div>
            </div>
        </div>
    `;
}

async function cancelOrder(id) {
    try {
        await api(`/api/orders/${id}/cancel`, { method: 'POST' });
        toast('Order cancelled');
        await Promise.all([loadOrders(), loadProducts(), loadDashboard()]);
        await openOrderDetails(id);
    } catch (error) {
        toast(error.message, 'error');
    }
}

function startOrderPolling(order) {
    clearTimeout(state.orderPoll);
    if (order.status !== 'PendingPayment') return;

    state.orderPoll = setTimeout(async () => {
        try {
            const latest = await api(`/api/orders/${order.id}`);
            if (!document.getElementById('detailsModal').classList.contains('open')) return;
            document.getElementById('orderDetails').innerHTML = orderDetailsHtml(latest);
            startOrderPolling(latest);
        } catch {
        }
    }, 2500);
}

async function loadDashboard() {
    try {
        const orders = await api('/api/orders');
        const paid = orders.filter(order => order.status === 'Paid');
        const pending = orders.filter(order => order.status === 'PendingPayment');
        const revenue = paid.reduce((sum, order) => sum + order.totalAmount, 0);

        document.getElementById('statOrders').textContent = orders.length;
        document.getElementById('statPaid').textContent = paid.length;
        document.getElementById('statPending').textContent = pending.length;
        document.getElementById('statRevenue').textContent = money(revenue);

        document.getElementById('recentOrders').innerHTML = orders.length ? `
            <table class="table">
                <thead><tr><th>Order</th><th>Customer</th><th>Total</th><th>Status</th><th>Created</th></tr></thead>
                <tbody>${orders.slice(0, 8).map(order => `
                    <tr>
                        <td><strong>#${order.id.slice(0, 8)}</strong></td>
                        <td>${escapeHtml(order.customerName)}</td>
                        <td>${money(order.totalAmount)}</td>
                        <td><span class="badge ${statusClass(order.status)}">${escapeHtml(order.status === 'PendingPayment' ? 'Pending payment' : order.status)}</span></td>
                        <td>${formatDate(order.createdAt)}</td>
                    </tr>
                `).join('')}</tbody>
            </table>
        ` : '<div class="empty">No orders yet.</div>';
    } catch (error) {
        toast(error.message, 'error');
    }
}

async function connectRealtime() {
    try {
        const connection = new signalR.HubConnectionBuilder()
            .withUrl(`${NOTIFY}/notifications`)
            .withAutomaticReconnect()
            .build();

        connection.on('ReceivePaymentUpdate', data => {
            document.getElementById('lastEvent').textContent = `${data.status} · ${money(data.amount)}`;
            document.getElementById('lastEventTime').textContent = `Order #${String(data.orderId).slice(0, 8)} · ${formatDate(data.timestamp)}`;
            toast(`Payment event: ${data.status}`, data.status === 'Failed' ? 'error' : 'success');
            loadDashboard();
            loadOrders();
        });

        connection.onreconnecting(() => {
            document.getElementById('liveStatus').textContent = 'Reconnecting…';
            document.getElementById('liveIndicator').style.background = 'var(--warm)';
        });

        connection.onreconnected(() => {
            document.getElementById('liveStatus').textContent = 'Realtime connected';
            document.getElementById('liveIndicator').style.background = 'var(--good)';
        });

        await connection.start();
        document.getElementById('liveStatus').textContent = 'Realtime connected';
    } catch {
        document.getElementById('liveStatus').textContent = 'Realtime unavailable';
        document.getElementById('liveIndicator').style.background = 'var(--danger)';
        setTimeout(connectRealtime, 5000);
    }
}

async function boot() {
    showPage('dashboard');
    await refreshAll();
    await connectRealtime();
}

boot();
