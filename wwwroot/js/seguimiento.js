document.addEventListener('DOMContentLoaded', () => {
    const appConfig = window.seguimientoApp || {};
    const requestCodeButton = document.getElementById('requestVerificationCodeButton');
    const validateCodeButton = document.getElementById('validateVerificationCodeButton');
    const dniInput = document.getElementById('dniAlumno');
    const emailInput = document.getElementById('correoSeguimiento');
    const verificationSection = document.getElementById('verificationSection');
    const codeInput = document.getElementById('verificationCodeInput');
    const feedback = document.getElementById('seguimientoClientFeedback');

    if (!requestCodeButton || !validateCodeButton || !dniInput || !emailInput || !codeInput) {
        return;
    }

    const setFeedback = (message, type = 'success') => {
        if (!message) {
            feedback.hidden = true;
            feedback.textContent = '';
            feedback.className = 'status-box';
            return;
        }

        feedback.hidden = false;
        feedback.textContent = message;
        feedback.className = `status-box ${type === 'success' ? 'status-success' : 'status-error'}`;
    };

    const readJson = async response => {
        const text = await response.text();
        if (!text) {
            return {};
        }

        try {
            return JSON.parse(text);
        } catch {
            return {};
        }
    };

    const normalizeDocument = value => (value || '').trim().toUpperCase().replace(/[^A-Z0-9-]/g, '').slice(0, 20);
    const normalizeEmail = value => (value || '').trim().toLowerCase();
    const isValidEmail = value => /^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(value);
    const isValidDocument = value => /^[A-Z0-9-]{3,20}$/.test(value);

    requestCodeButton.addEventListener('click', async () => {
        const dni = normalizeDocument(dniInput.value);
        const correo = normalizeEmail(emailInput.value);

        dniInput.value = dni;
        emailInput.value = correo;

        if (!isValidDocument(dni)) {
            setFeedback('Ingresa un documento valido de 3 a 20 caracteres.', 'error');
            return;
        }

        if (!isValidEmail(correo)) {
            setFeedback('Ingresa el correo con el que se realizo la inscripcion.', 'error');
            return;
        }

        requestCodeButton.disabled = true;
        requestCodeButton.textContent = 'Enviando codigo...';

        try {
            const response = await fetch(appConfig.solicitarCodigoUrl, {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify({ dniAlumno: dni, correo })
            });

            const data = await readJson(response);
            if (!response.ok || !data.exito) {
                throw new Error(data.mensaje || 'No se pudo enviar el codigo de verificacion.');
            }

            setFeedback(data.mensaje, 'success');
            verificationSection.classList.remove('hidden-field');
            codeInput.focus();
        } catch (error) {
            setFeedback(error.message || 'No se pudo enviar el codigo de verificacion.', 'error');
        } finally {
            requestCodeButton.disabled = false;
            requestCodeButton.textContent = 'Enviar codigo';
        }
    });

    validateCodeButton.addEventListener('click', async () => {
        const dni = normalizeDocument(dniInput.value);
        const correo = normalizeEmail(emailInput.value);
        const codigo = (codeInput.value || '').trim().replace(/\D/g, '');

        dniInput.value = dni;
        emailInput.value = correo;
        codeInput.value = codigo;

        if (!isValidDocument(dni)) {
            setFeedback('Ingresa un documento valido de 3 a 20 caracteres.', 'error');
            return;
        }

        if (!isValidEmail(correo)) {
            setFeedback('Ingresa el correo con el que se realizo la inscripcion.', 'error');
            return;
        }

        if (codigo.length !== 6) {
            setFeedback('Ingresa el codigo completo de 6 digitos.', 'error');
            return;
        }

        validateCodeButton.disabled = true;
        validateCodeButton.textContent = 'Validando...';

        try {
            const response = await fetch(appConfig.validarCodigoUrl, {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify({ dniAlumno: dni, correo, codigo })
            });

            const data = await readJson(response);
            if (!response.ok || !data.exito) {
                throw new Error(data.mensaje || 'No se pudo validar el codigo.');
            }

            window.location.href = data.redirectUrl;
        } catch (error) {
            setFeedback(error.message || 'No se pudo validar el codigo.', 'error');
        } finally {
            validateCodeButton.disabled = false;
            validateCodeButton.textContent = 'Ingresar al dashboard';
        }
    });
});
