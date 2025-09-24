
const axios = require('axios');
const { connect, StringCodec, consumerOpts, AckPolicy, DeliverPolicy } = require('nats');
const assert = require('assert');

// --- Configuration ---
const BASE_URL = 'http://localhost:5000';
const NATS_URL = 'nats://localhost:4222';
const apiClient = axios.create({
    baseURL: BASE_URL,
    timeout: 10000,
});

// --- Helper Functions ---
const generateRandomUser = () => {
    const randomId = Math.random().toString(36).substring(2, 15);
    return {
        email: `testuser_${randomId}@example.com`,
        password: `Password_${randomId}!`,
    };
};

const logStep = (message) => console.log(`\n--- ${message} ---`);
const logSuccess = (message) => console.log(`✅ ${message}`);
const logError = (error) => console.error(`❌ Error: ${error.message}`, error.response ? error.response.data : '');

const makeRequest = async (config) => {
    try {
        const response = await apiClient(config);
        logSuccess(`${config.method.toUpperCase()} ${config.url} successful.`);
        return response.data;
    } catch (error) {
        logError(error);
        throw error;
    }
};

const sleep = (ms) => new Promise(resolve => setTimeout(resolve, ms));

const waitForNatsEvent = async (js, subject) => {
    let consumer;
    // Retry mechanism to handle the race condition of stream creation
    for (let i = 0; i < 5; i++) {
        try {
            consumer = await js.consumers.get("AUTH_EVENTS", {
                deliver_policy: DeliverPolicy.New,
                ack_policy: AckPolicy.Explicit,
            });
            logSuccess("NATS consumer created successfully.");
            break; // Success
        } catch (err) {
            if (err.api_error?.err_code === 10059) { // Stream not found
                logSuccess(`Stream not found, retrying in 1 second... (${i + 1}/5)`);
                await sleep(1000);
            } else {
                throw err;
            }
        }
    }

    if (!consumer) {
        throw new Error("Could not create NATS consumer after multiple retries. The stream may not have been created by the service.");
    }

    const messages = await consumer.consume({ max_messages: 1, expires: 8000 });
    for await (const m of messages) {
        const sc = StringCodec();
        const eventData = JSON.parse(sc.decode(m.data));
        logSuccess(`Received NATS event on subject: ${subject}`);
        console.log('Event data:', eventData);
        m.ack();
        return eventData;
    }
};


// --- API Service ---
const authService = {
    register: (userData) => makeRequest({ method: 'post', url: '/api/auth/register', data: userData }),
    login: (credentials) => makeRequest({ method: 'post', url: '/api/auth/login', data: credentials }),
    validateToken: (token) => makeRequest({ method: 'get', url: '/api/auth/validate', headers: { Authorization: `Bearer ${token}` } }),
    refreshToken: (refreshToken) => makeRequest({ method: 'post', url: '/api/auth/refresh', data: { refreshToken } }),
    forgotPassword: (email) => makeRequest({ method: 'post', url: '/api/auth/forgot-password', data: { email } }),
    resetPassword: (resetData) => makeRequest({ method: 'post', url: '/api/auth/reset-password', data: resetData }),
};

// --- Test Lifecycle ---
const runAuthLifecycleTest = async () => {
    logStep('Starting Auth Lifecycle Test');
    const user = generateRandomUser();
    let accessToken = '';
    let refreshToken = '';
    let natsConnection;

    try {
        // Connect to NATS
        logStep('Connecting to NATS server');
        natsConnection = await connect({ servers: NATS_URL });
        const js = natsConnection.jetstream();
        logSuccess('Connected to NATS and got JetStream context');

        // 1. Register a new user and listen for the event
        logStep('Registering a new user');
        const registerEventPromise = waitForNatsEvent(js, 'UserRegisteredEvent');
        await authService.register(user);
        console.log(`User created: ${user.email}`);
        await registerEventPromise;

        // 2. Log in with the new user and listen for the event
        logStep('Logging in');
        const loginEventPromise = waitForNatsEvent(js, 'UserLoggedInEvent');
        const loginResponse = await authService.login(user);
        accessToken = loginResponse.token;
        refreshToken = loginResponse.refreshToken;
        console.log('Login successful, tokens received.');
        await loginEventPromise;

        // 3. Validate the initial token
        logStep('Validating access token');
        await authService.validateToken(accessToken);
        console.log('Token is valid.');

        // 4. Refresh the token
        logStep('Refreshing token');
        const refreshResponse = await authService.refreshToken(refreshToken);
        accessToken = refreshResponse.token;
        refreshToken = refreshResponse.refreshToken;
        console.log('Token refreshed.');

        // 5. Validate the new token
        logStep('Validating new access token');
        await authService.validateToken(accessToken);
        console.log('New token is valid.');

        // 6. Forgot Password - Listen for event to get token
        logStep('Forgot Password');
        const passwordResetEventPromise = waitForNatsEvent(js, 'PasswordResetRequestedEvent');
        await authService.forgotPassword(user.email);
        const passwordResetEvent = await passwordResetEventPromise;

        // --- ASSERTION ---
        logStep('Asserting PasswordResetRequestedEvent');
        assert(passwordResetEvent, 'PasswordResetRequestedEvent was not received.');
        assert(passwordResetEvent.Token, 'Event data does not contain a Token.');
        assert(typeof passwordResetEvent.Token === 'string', 'Token is not a string.');
        assert(passwordResetEvent.Token.length > 0, 'Token is an empty string.');
        logSuccess('PasswordResetRequestedEvent is valid.');

        const resetToken = passwordResetEvent.Token;
        console.log('Password reset initiated, token received from event.');

        // 7. Reset Password
        logStep('Reset Password');
        const newPassword = `NewPassword_${Math.random().toString(36).substring(2, 15)}!`;
        await authService.resetPassword({ email: user.email, token: resetToken, newPassword });
        console.log('Password has been reset.');

        // 8. Log in with the new password
        logStep('Logging in with new password');
        const newLoginResponse = await authService.login({ email: user.email, password: newPassword });
        accessToken = newLoginResponse.token;
        console.log('Login with new password successful.');

        // 9. Validate the token after password reset
        logStep('Validating token after password reset');
        await authService.validateToken(accessToken);
        console.log('Token is valid after password reset.');

        logStep('Auth Lifecycle Test Completed Successfully');
    } catch (error) {
        logStep('Auth Lifecycle Test Failed');
    } finally {
        if (natsConnection) {
            await natsConnection.close();
            logSuccess('NATS connection closed.');
        }
    }
};

runAuthLifecycleTest();
