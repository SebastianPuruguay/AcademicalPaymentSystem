document.addEventListener('DOMContentLoaded', () => {
    const sortAlpha = items => [...items].sort((left, right) => left.localeCompare(right, 'es'));

    const universidades = sortAlpha([
        'PONTIFICIA UNIVERSIDAD CATOLICA DEL PERU',
        'UNIVERSIDAD NACIONAL MAYOR DE SAN MARCOS',
        'UNIVERSIDAD PERUANA CAYETANO HEREDIA',
        'UNIVERSIDAD NACIONAL DE INGENIERIA',
        'UNIVERSIDAD NACIONAL AGRARIA LA MOLINA',
        'UNIVERSIDAD DEL PACIFICO',
        'UNIVERSIDAD DE LIMA',
        'UNIVERSIDAD DE PIURA',
        'UNIVERSIDAD PERUANA DE CIENCIAS APLICADAS',
        'UNIVERSIDAD DE SAN MARTIN DE PORRES',
        'UNIVERSIDAD CIENTIFICA DEL SUR',
        'UNIVERSIDAD SAN IGNACIO DE LOYOLA',
        'UNIVERSIDAD NACIONAL DE SAN AGUSTIN DE AREQUIPA',
        'UNIVERSIDAD ESAN',
        'UNIVERSIDAD RICARDO PALMA',
        'UNIVERSIDAD NACIONAL DE SAN ANTONIO ABAD DEL CUSCO',
        'UNIVERSIDAD PRIVADA ANTENOR ORREGO',
        'UNIVERSIDAD CONTINENTAL',
        'UNIVERSIDAD PRIVADA DEL NORTE',
        'UNIVERSIDAD CESAR VALLEJO',
        'UNIVERSIDAD CATOLICA SAN PABLO',
        'UNIVERSIDAD NACIONAL DE TRUJILLO',
        'UNIVERSIDAD TECNOLOGICA DEL PERU',
        'UNIVERSIDAD ANDINA DEL CUSCO',
        'UNIVERSIDAD CATOLICA DE SANTA MARIA',
        'UNIVERSIDAD PRIVADA DE TACNA',
        'UNIVERSIDAD NACIONAL DEL ALTIPLANO',
        'UNIVERSIDAD SENOR DE SIPAN',
        'UNIVERSIDAD NACIONAL DE PIURA',
        'UNIVERSIDAD ALAS PERUANAS',
        'UNIVERSIDAD FEDERICO VILLARREAL'
    ]);

    const paises = [
        'AFGANISTAN', 'ALBANIA', 'ALEMANIA', 'ANDORRA', 'ANGOLA', 'ANTIGUA Y BARBUDA', 'ARABIA SAUDITA', 'ARGELIA', 'ARGENTINA', 'ARMENIA',
        'AUSTRALIA', 'AUSTRIA', 'AZERBAIYAN', 'BAHAMAS', 'BANGLADES', 'BARBADOS', 'BAREIN', 'BELGICA', 'BELICE', 'BENIN',
        'BIELORRUSIA', 'BIRMANIA', 'BOLIVIA', 'BOSNIA Y HERZEGOVINA', 'BOTSUANA', 'BRASIL', 'BRUNEI', 'BULGARIA', 'BURKINA FASO', 'BURUNDI',
        'BUTAN', 'CABO VERDE', 'CAMBOYA', 'CAMERUN', 'CANADA', 'CATAR', 'CHAD', 'CHILE', 'CHINA', 'CHIPRE',
        'CIUDAD DEL VATICANO', 'COLOMBIA', 'COMORAS', 'COREA DEL NORTE', 'COREA DEL SUR', 'COSTA DE MARFIL', 'COSTA RICA', 'CROACIA', 'CUBA', 'DINAMARCA',
        'DOMINICA', 'ECUADOR', 'EGIPTO', 'EL SALVADOR', 'EMIRATOS ARABES UNIDOS', 'ERITREA', 'ESLOVAQUIA', 'ESLOVENIA', 'ESPANA', 'ESTADOS UNIDOS',
        'ESTONIA', 'ETIOPIA', 'FILIPINAS', 'FINLANDIA', 'FIYI', 'FRANCIA', 'GABON', 'GAMBIA', 'GEORGIA', 'GHANA',
        'GRANADA', 'GRECIA', 'GUATEMALA', 'GUYANA', 'GUINEA', 'GUINEA ECUATORIAL', 'GUINEA-BISAU', 'HAITI', 'HONDURAS', 'HUNGRIA',
        'INDIA', 'INDONESIA', 'IRAK', 'IRAN', 'IRLANDA', 'ISLANDIA', 'ISLAS MARSHALL', 'ISLAS SALOMON', 'ISRAEL', 'ITALIA',
        'JAMAICA', 'JAPON', 'JORDANIA', 'KAZAJISTAN', 'KENIA', 'KIRGUISTAN', 'KIRIBATI', 'KUWAIT', 'LAOS', 'LESOTO',
        'LETONIA', 'LIBANO', 'LIBERIA', 'LIBIA', 'LIECHTENSTEIN', 'LITUANIA', 'LUXEMBURGO', 'MACEDONIA DEL NORTE', 'MADAGASCAR', 'MALASIA',
        'MALAWI', 'MALDIVAS', 'MALI', 'MALTA', 'MARRUECOS', 'MAURICIO', 'MAURITANIA', 'MEXICO', 'MICRONESIA', 'MOLDAVIA',
        'MONACO', 'MONGOLIA', 'MONTENEGRO', 'MOZAMBIQUE', 'NAMIBIA', 'NAURU', 'NEPAL', 'NICARAGUA', 'NIGER', 'NIGERIA',
        'NORUEGA', 'NUEVA ZELANDA', 'OMAN', 'PAISES BAJOS', 'PAKISTAN', 'PALAOS', 'PANAMA', 'PAPUA NUEVA GUINEA', 'PARAGUAY', 'PERU',
        'POLONIA', 'PORTUGAL', 'REINO UNIDO', 'REPUBLICA CENTROAFRICANA', 'REPUBLICA CHECA', 'REPUBLICA DEL CONGO', 'REPUBLICA DEMOCRATICA DEL CONGO', 'REPUBLICA DOMINICANA',
        'RUANDA', 'RUMANIA', 'RUSIA', 'SAMOA', 'SAN CRISTOBAL Y NIEVES', 'SAN MARINO', 'SAN VICENTE Y LAS GRANADINAS', 'SANTA LUCIA',
        'SANTO TOME Y PRINCIPE', 'SENEGAL', 'SERBIA', 'SEYCHELLES', 'SIERRA LEONA', 'SINGAPUR', 'SIRIA', 'SOMALIA', 'SRI LANKA',
        'SUAZILANDIA', 'SUDAFRICA', 'SUDAN', 'SUDAN DEL SUR', 'SUECIA', 'SUIZA', 'SURINAM', 'TAILANDIA', 'TANZANIA', 'TAYIKISTAN',
        'TIMOR ORIENTAL', 'TOGO', 'TONGA', 'TRINIDAD Y TOBAGO', 'TUNEZ', 'TURKMENISTAN', 'TURQUIA', 'TUVALU', 'UCRANIA', 'UGANDA',
        'URUGUAY', 'UZBEKISTAN', 'VANUATU', 'VENEZUELA', 'VIETNAM', 'YEMEN', 'YIBUTI', 'ZAMBIA', 'ZIMBABUE'
    ];

    const departamentosPeru = [
        'AMAZONAS', 'ANCASH', 'APURIMAC', 'AREQUIPA', 'AYACUCHO', 'CAJAMARCA', 'CALLAO', 'CUSCO', 'HUANCAVELICA', 'HUANUCO',
        'ICA', 'JUNIN', 'LA LIBERTAD', 'LAMBAYEQUE', 'LIMA', 'LORETO', 'MADRE DE DIOS', 'MOQUEGUA', 'PASCO', 'PIURA',
        'PUNO', 'SAN MARTIN', 'TACNA', 'TUMBES', 'UCAYALI'
    ];

    const countryCallingCodeGroups = [
        {
            label: 'America',
            countries: [
                { name: 'Estados Unidos', key: 'ESTADOS_UNIDOS', code: '+1' },
                { name: 'Canada', key: 'CANADA', code: '+1' },
                { name: 'Mexico', key: 'MEXICO', code: '+52' },
                { name: 'Brasil', key: 'BRASIL', code: '+55' },
                { name: 'Argentina', key: 'ARGENTINA', code: '+54' },
                { name: 'Colombia', key: 'COLOMBIA', code: '+57' },
                { name: 'Chile', key: 'CHILE', code: '+56' },
                { name: 'Venezuela', key: 'VENEZUELA', code: '+58' },
                { name: 'Peru', key: 'PERU', code: '+51' },
                { name: 'Ecuador', key: 'ECUADOR', code: '+593' },
                { name: 'Cuba', key: 'CUBA', code: '+53' },
                { name: 'Bolivia', key: 'BOLIVIA', code: '+591' },
                { name: 'Costa Rica', key: 'COSTA_RICA', code: '+506' },
                { name: 'Panama', key: 'PANAMA', code: '+507' },
                { name: 'Uruguay', key: 'URUGUAY', code: '+598' }
            ]
        },
        {
            label: 'Europa',
            countries: [
                { name: 'Espana', key: 'ESPANA', code: '+34' },
                { name: 'Alemania', key: 'ALEMANIA', code: '+49' },
                { name: 'Francia', key: 'FRANCIA', code: '+33' },
                { name: 'Italia', key: 'ITALIA', code: '+39' },
                { name: 'Reino Unido', key: 'REINO_UNIDO', code: '+44' },
                { name: 'Rusia', key: 'RUSIA', code: '+7' },
                { name: 'Ucrania', key: 'UCRANIA', code: '+380' },
                { name: 'Polonia', key: 'POLONIA', code: '+48' },
                { name: 'Rumania', key: 'RUMANIA', code: '+40' },
                { name: 'Paises Bajos', key: 'PAISES_BAJOS', code: '+31' },
                { name: 'Belgica', key: 'BELGICA', code: '+32' },
                { name: 'Grecia', key: 'GRECIA', code: '+30' },
                { name: 'Portugal', key: 'PORTUGAL', code: '+351' },
                { name: 'Suecia', key: 'SUECIA', code: '+46' },
                { name: 'Noruega', key: 'NORUEGA', code: '+47' }
            ]
        },
        {
            label: 'Asia',
            countries: [
                { name: 'China', key: 'CHINA', code: '+86' },
                { name: 'India', key: 'INDIA', code: '+91' },
                { name: 'Japon', key: 'JAPON', code: '+81' },
                { name: 'Corea del Sur', key: 'COREA_DEL_SUR', code: '+82' },
                { name: 'Indonesia', key: 'INDONESIA', code: '+62' },
                { name: 'Turquia', key: 'TURQUIA', code: '+90' },
                { name: 'Filipinas', key: 'FILIPINAS', code: '+63' },
                { name: 'Tailandia', key: 'TAILANDIA', code: '+66' },
                { name: 'Vietnam', key: 'VIETNAM', code: '+84' },
                { name: 'Israel', key: 'ISRAEL', code: '+972' },
                { name: 'Malasia', key: 'MALASIA', code: '+60' },
                { name: 'Singapur', key: 'SINGAPUR', code: '+65' },
                { name: 'Pakistan', key: 'PAKISTAN', code: '+92' },
                { name: 'Banglades', key: 'BANGLADES', code: '+880' },
                { name: 'Arabia Saudita', key: 'ARABIA_SAUDITA', code: '+966' }
            ]
        },
        {
            label: 'Africa',
            countries: [
                { name: 'Egipto', key: 'EGIPTO', code: '+20' },
                { name: 'Sudafrica', key: 'SUDAFRICA', code: '+27' },
                { name: 'Nigeria', key: 'NIGERIA', code: '+234' },
                { name: 'Kenia', key: 'KENIA', code: '+254' },
                { name: 'Marruecos', key: 'MARRUECOS', code: '+212' },
                { name: 'Argelia', key: 'ARGELIA', code: '+213' },
                { name: 'Uganda', key: 'UGANDA', code: '+256' },
                { name: 'Ghana', key: 'GHANA', code: '+233' },
                { name: 'Camerun', key: 'CAMERUN', code: '+237' },
                { name: 'Costa de Marfil', key: 'COSTA_DE_MARFIL', code: '+225' },
                { name: 'Senegal', key: 'SENEGAL', code: '+221' },
                { name: 'Tanzania', key: 'TANZANIA', code: '+255' },
                { name: 'Sudan', key: 'SUDAN', code: '+249' },
                { name: 'Libia', key: 'LIBIA', code: '+218' },
                { name: 'Tunez', key: 'TUNEZ', code: '+216' }
            ]
        },
        {
            label: 'Oceania',
            countries: [
                { name: 'Australia', key: 'AUSTRALIA', code: '+61' },
                { name: 'Nueva Zelanda', key: 'NUEVA_ZELANDA', code: '+64' },
                { name: 'Fiji', key: 'FIYI', code: '+679' },
                { name: 'Papua Nueva Guinea', key: 'PAPUA_NUEVA_GUINEA', code: '+675' },
                { name: 'Tonga', key: 'TONGA', code: '+676' }
            ]
        },
        {
            label: 'Medio Oriente',
            countries: [
                { name: 'Iran', key: 'IRAN', code: '+98' },
                { name: 'Iraq', key: 'IRAK', code: '+964' },
                { name: 'Jordania', key: 'JORDANIA', code: '+962' },
                { name: 'Libano', key: 'LIBANO', code: '+961' },
                { name: 'Kuwait', key: 'KUWAIT', code: '+965' },
                { name: 'Emiratos Arabes Unidos', key: 'EMIRATOS_ARABES_UNIDOS', code: '+971' },
                { name: 'Oman', key: 'OMAN', code: '+968' },
                { name: 'Catar', key: 'CATAR', code: '+974' },
                { name: 'Bahrein', key: 'BAREIN', code: '+973' },
                { name: 'Yemen', key: 'YEMEN', code: '+967' }
            ]
        }
    ];

    const countryCallingCodes = countryCallingCodeGroups.reduce((map, group) => {
        group.countries.forEach(country => {
            map[country.key] = country.code;
        });
        return map;
    }, {});

    const profesiones = sortAlpha([
        'ADMINISTRACION DE EMPRESAS', 'ADMINISTRACION EN TURISMO', 'ADMINISTRACION EN SALUD','AGRONOMIA', 'ANALISIS DE DATOS', 'ANTROPOLOGIA', 'ARQUITECTURA',
        'ARTE Y DISENO GRAFICO', 'BIOLOGIA', 'BIOTECNOLOGIA', 'BIBLIOTECOLOGIA Y CIENCIAS DE LA INFORMACION', 'CIENCIAS DE LA COMPUTACION',
        'CIENCIAS DE LA COMUNICACION', 'CIENCIAS DEL DEPORTE', 'CIENCIAS POLITICAS', 'CINE Y TELEVISION', 'COMUNICACION SOCIAL',
        'CONTABILIDAD', 'DANZA', 'DERECHO', 'DISENO DE INTERIORES', 'DISENO DE MODA', 'DISENO DE PRODUCTOS', 'ECONOMIA',
        'EDUCACION FISICA', 'EDUCACION INICIAL', 'EDUCACION PRIMARIA', 'EDUCACION SECUNDARIA', 'ENFERMERIA', 'ESTADISTICA',
        'ESTADISTICA E INFORMATICA', 'FARMACIA Y BIOQUIMICA', 'FILOSOFIA', 'FISICA', 'FISIOTERAPIA', 'GASTRONOMIA',
        'GESTION CULTURAL', 'HISTORIA', 'INGENIERIA AMBIENTAL', 'INGENIERIA BIOMEDICA', 'INGENIERIA CIVIL', 'INGENIERIA DE ALIMENTOS',
        'INGENIERIA DE MINAS', 'INGENIERIA DE SISTEMAS', 'INGENIERIA DE SOFTWARE', 'INGENIERIA DE TELECOMUNICACIONES',
        'INGENIERIA ELECTRONICA', 'INGENIERIA ELECTRICA', 'INGENIERIA GEOLOGICA', 'INGENIERIA INDUSTRIAL', 'INGENIERIA MECANICA',
        'INGENIERIA MECATRONICA', 'INGENIERIA METALURGICA', 'INGENIERIA QUIMICA', 'INGENIERIA TEXTIL', 'LABORATORIO CLINICO',
        'LITERATURA', 'LINGUISTICA', 'MARKETING', 'MATEMATICAS', 'MATEMATICAS APLICADAS', 'MEDICINA HUMANA',
        'MEDICINA VETERINARIA', 'MUSICA', 'NUTRICION Y DIETETICA', 'OBSTETRICIA', 'ODONTOLOGIA', 'OPTOMETRIA', 'PSICOLOGIA',
        'PUBLICIDAD', 'QUIMICA', 'RELACIONES INTERNACIONALES', 'SOCIOLOGIA', 'TEATRO', 'TECNICO EN ADMINISTRACION', 'TECNOLOGO MEDICO',
        'TECNICO EN COMERCIO EXTERIOR', 'TECNICO EN COMPUTACION E INFORMATICA', 'TECNICO EN CONTABILIDAD', 'TECNICO EN ELECTRICIDAD',
        'TECNICO EN ELECTRONICA INDUSTRIAL', 'TECNICO EN ENFERMERIA', 'TECNICO EN FARMACIA', 'TECNICO EN FISIOTERAPIA',
        'TECNICO EN GASTRONOMIA', 'TECNICO EN HOTELERIA Y TURISMO', 'TECNICO EN INSTRUMENTACION QUIRURGICA','TECNICO EN RADIOLOGIA E IMAGENOLOGIA',
        'TECNICO EN LABORATORIO CLINICO', 'TECNICO EN LOGISTICA', 'TECNICO EN MARKETING', 'TECNICO EN MECANICA AUTOMOTRIZ',
        'TECNICO EN OPTOMETRIA', 'TECNICO EN RADIOLOGIA', 'TECNICO EN TELECOMUNICACIONES', 'TERAPIA FISICA Y REHABILITACION',
        'TERAPIA OCUPACIONAL', 'TRABAJO SOCIAL', 'TRADUCCION E INTERPRETACION', 'TURISMO Y HOTELERIA', 'ZOOTECNIA',
        'NO PROFESIONAL/ ESTUDIANTE', 'OTROS',
    ]);

    const especialidadesMedicina = sortAlpha([
        'ADMINISTRACION EN SALUD', 'ALERGOLOGIA', 'ALGOLOGIA', 'ANALISIS CLINICO', 'ANATOMIA PATOLOGICA', 'ANESTESIOLOGIA', 'ANGIOLOGIA',
        'AUDITORIA MEDICA', 'BIOQUIMICA CLINICA', 'CARDIOLOGIA', 'CIRUGIA CARDIACA', 'CIRUGIA CRANEOFACIAL', 'CIRUGIA GENERAL',
        'CIRUGIA ONCOLOGICA', 'CIRUGIA ORAL Y MAXILOFACIAL', 'CIRUGIA ORTOPEDICA', 'CIRUGIA PEDIATRICA', 'CIRUGIA PLASTICA',
        'CIRUGIA TORACICA', 'CIRUGIA VASCULAR', 'COLOPROCTOLOGIA', 'DERMATOLOGIA', 'EMBRIOLOGIA', 'ENDOCRINOLOGIA', 'EPIDEMIOLOGIA',
        'ESTOMATOLOGIA', 'FARMACOLOGIA', 'FARMACOLOGIA CLINICA', 'FONIATRIA', 'GASTROENTEROLOGIA', 'GENETICA', 'GENETICA MEDICA',
        'GERIATRIA', 'GINECOLOGIA Y OBSTETRICIA O TOCOLOGIA', 'HEMATOLOGIA', 'HEPATOLOGIA', 'INFECTOLOGIA', 'INMUNOLOGIA',
        'MEDICINA AEROESPACIAL', 'MEDICINA DE EMERGENCIA', 'MEDICINA DEL DEPORTE', 'MEDICINA DEL TRABAJO', 'MEDICINA FAMILIAR Y COMUNITARIA',
        'MEDICINA FISICA Y REHABILITACION', 'MEDICINA FORENSE', 'MEDICINA INTENSIVA', 'MEDICINA INTERNA', 'MEDICINA NUCLEAR',
        'MEDICINA PALIATIVA', 'MEDICINA PREVENTIVA Y SALUD PUBLICA', 'MICROBIOLOGIA Y PARASITOLOGIA', 'NEFROLOGIA', 'NEUMOLOGIA',
        'NEUROCIRUGIA', 'NEUROFISIOLOGIA CLINICA', 'NEUROLOGIA', 'NUTRIOLOGIA', 'ODONTOLOGIA', 'OFTALMOLOGIA', 'ONCOLOGIA MEDICA',
        'ONCOLOGIA RADIOTERAPICA', 'OTORRINOLARINGOLOGIA', 'PEDIATRIA', 'PSIQUIATRIA', 'RADIOLOGIA', 'REUMATOLOGIA', 'SALUD PUBLICA',
        'TOXICOLOGIA', 'TRAUMATOLOGIA Y ORTOPEDIA', 'UROLOGIA'
    ]);

    const especialidadesEnfermeria = sortAlpha([
        'ENFERMERIA NEONATAL', 'ENFERMERIA PEDIATRICA', 'ENFERMERIA OBSTETRICO-GINECOLOGICA', 'ENFERMERIA GERIATRICA',
        'ENFERMERIA DE CUIDADOS PALIATIVOS', 'ENFERMERIA INTENSIVA', 'ENFERMERIA ONCOLOGICA', 'ENFERMERIA DE URGENCIAS',
        'ENFERMERIA FAMILIAR Y COMUNITARIA', 'ENFERMERIA ADMINISTRATIVA Y DE GESTION', 'ENFERMERIA CENTRO QUIRURGICO',
    ]);
    const especialidadesTecnologoMedico = sortAlpha([
        'TECNOLOGO MEDICO EN RADIOLOGIA',
        'TECNOLOGO MEDICO EN LABORATORIO CLINICO Y ANATOMIA PATOLOGICA',
        'TECNOLOGO MEDICO EN TERAPIA FISICA Y REHABILITACION',
        'TECNOLOGO MEDICO EN TERAPIA DE LENGUAJE',
        'TECNOLOGO MEDICO EN OPTOMETRIA',
        'TECNOLOGO MEDICO EN TERAPIA OCUPACIONAL',
        'TECNOLOGO MEDICO EN AUDIOLOGIA',
        'TECNOLOGO MEDICO EN IMAGENOLOGIA',
    ]);

    const appConfig = window.inscripcionApp || {};
    const selectedCourseRaw = document.getElementById('selectedCourseData')?.textContent?.trim() || 'null';
    const selectedCourse = JSON.parse(selectedCourseRaw);
    const restrictedDiplomadoCourseId = 217;
    const restrictedDiplomadoProfession = 'MEDICINA HUMANA';
    const restrictedDiplomadoSpecialty = 'ANESTESIOLOGIA';

    const selectionForm = document.querySelector('.selection-form');
    if (selectionForm) {
        const courseSelect = selectionForm.querySelector('select[name="cursoId"]');
        const continueButton = selectionForm.querySelector('button[type="submit"]');

        if (courseSelect && continueButton) {
            const wrapper = document.createElement('div');
            wrapper.className = 'searchable-select';
            const input = document.createElement('input');
            input.type = 'search';
            input.className = 'searchable-select-input';
            input.autocomplete = 'off';
            input.placeholder = courseSelect.dataset.searchPlaceholder || 'Buscar...';
            const results = document.createElement('div');
            results.className = 'searchable-select-results';
            results.hidden = true;

            courseSelect.parentNode.insertBefore(wrapper, courseSelect);
            wrapper.appendChild(input);
            wrapper.appendChild(courseSelect);
            wrapper.appendChild(results);

            const normalize = value => (value || '')
                .toString()
                .normalize('NFD')
                .replace(/[\u0300-\u036f]/g, '')
                .toLowerCase()
                .trim();

            const refreshInput = () => {
                const selectedOption = courseSelect.options[courseSelect.selectedIndex];
                input.value = selectedOption && selectedOption.value ? selectedOption.textContent.trim() : '';
            };

            const renderOptions = () => {
                const query = normalize(input.value);
                const matches = Array.from(courseSelect.options)
                    .filter(option => option.value)
                    .filter(option => !query || normalize(option.textContent).includes(query));

                results.innerHTML = '';
                if (matches.length === 0) {
                    const empty = document.createElement('div');
                    empty.className = 'searchable-select-empty';
                    empty.textContent = 'No se encontraron cursos.';
                    results.appendChild(empty);
                    results.hidden = false;
                    return;
                }

                matches.forEach(option => {
                    const item = document.createElement('div');
                    item.className = 'searchable-select-option';
                    item.tabIndex = 0;
                    item.textContent = option.textContent;
                    const selectItem = () => {
                        courseSelect.value = option.value;
                        refreshInput();
                        results.hidden = true;
                        courseSelect.dispatchEvent(new Event('change', { bubbles: true }));
                    };
                    item.addEventListener('mousedown', event => event.preventDefault());
                    item.addEventListener('click', selectItem);
                    item.addEventListener('keydown', event => {
                        if (event.key === 'Enter' || event.key === ' ') {
                            event.preventDefault();
                            selectItem();
                        }
                    });
                    results.appendChild(item);
                });
                results.hidden = false;
            };

            const syncSelection = () => {
                continueButton.disabled = !courseSelect.value;
            };

            input.addEventListener('input', () => {
                courseSelect.value = '';
                syncSelection();
                renderOptions();
            });
            input.addEventListener('focus', renderOptions);
            input.addEventListener('blur', () => {
                window.setTimeout(() => {
                    results.hidden = true;
                    refreshInput();
                }, 120);
            });
            syncSelection();
            refreshInput();
            courseSelect.addEventListener('change', syncSelection);
        }

        return;
    }

    const form = document.getElementById('inscripcionForm');
    if (!form || !selectedCourse) {
        return;
    }

    const feedback = document.getElementById('formFeedback');
    const submitButton = document.getElementById('submitButton');
    const confirmRegistrationButton = document.getElementById('confirmRegistrationButton');
    const backToFormButton = document.getElementById('backToFormButton');
    const successPanel = document.getElementById('successPanel');
    const successMessage = document.getElementById('successMessage');
    const successPrimaryLink = document.getElementById('successPrimaryLink');
    const successTrackingLink = document.getElementById('successTrackingLink');
    const paymentPlanSection = document.getElementById('paymentPlanSection');
    const paymentPlanIntro = document.getElementById('paymentPlanIntro');
    const paymentFinalCost = document.getElementById('paymentFinalCost');
    const paymentMaxInstallments = document.getElementById('paymentMaxInstallments');
    const installmentsInput = document.getElementById('numeroCuotas');
    const paymentScheduleBody = document.getElementById('paymentScheduleBody');
    const typeDocumentInput = document.getElementById('tipoDocumento');
    const documentNumberInput = document.getElementById('numeroDocumento');
    const namesInput = document.getElementById('nombres');
    const lastNamesInput = document.getElementById('apellidos');
    const dniLoader = document.getElementById('dniLoader');
    const professionInput = document.getElementById('profesion');
    const specialtyContainer = document.getElementById('especialidadContainer');
    const specialtyInput = document.getElementById('especialidad');
    const countryInput = document.getElementById('pais');
    const regionPeruGroup = document.getElementById('regionPeruGroup');
    const regionPeruInput = document.getElementById('regionPeru');
    const regionOutsideGroup = document.getElementById('regionExteriorGroup');
    const regionOutsideInput = document.getElementById('regionExterior');
    const regionHiddenInput = document.getElementById('region');
    const emailInput = document.getElementById('correo');
    const emailConfirmInput = document.getElementById('confirmacionCorreo');
    const countryCodeInput = document.getElementById('codigoPais');
    const phoneInput = document.getElementById('celular');
    const institutionTypeInput = document.getElementById('tipoInstitucion');
    const institutionNameInput = document.getElementById('nombreInstitucion');
    const ipressContainer = document.getElementById('ipressContainer');
    const ipressInput = document.getElementById('nombreIpress');
    const universityContainer = document.getElementById('universidadContainer');
    const universityInput = document.getElementById('listaUniversidades');
    const otherInstitutionContainer = document.getElementById('otraInstitucionContainer');
    const otherInstitutionInput = document.getElementById('nombreOtraInstitucion');
    const insnsbConditionGroup = document.getElementById('insnsbConditionGroup');
    const insnsbConditionInput = document.getElementById('condicionLaboralInsnsb');
    const insnsbCodeGroup = document.getElementById('insnsbCodeGroup');
    const insnsbCodeInput = document.getElementById('codigoInsnsb');
    const insnsbCodeHelperText = insnsbCodeGroup?.querySelector('.helper-text');
    const communicationInput = document.getElementById('medioComunicacion');
    const otherMediaGroup = document.getElementById('otherMediaGroup');
    const otherMediaInput = document.getElementById('otroMedioComunicacion');
    const dataPolicyInput = document.getElementById('aceptaTratamientoDatos');
    const internalCost = document.getElementById('courseInternalCost');
    const paymentSummaryCards = document.querySelectorAll('.js-payment-summary-card');

    let cronogramaActual = [];

    const hasValue = value => value !== null && value !== undefined && value !== '';
    const isRestrictedDiplomado = () => Number(appConfig.selectedCourseId ?? selectedCourse?.idActividad ?? 0) === restrictedDiplomadoCourseId;
    const normalizeEmail = value => (value || '').trim().toLowerCase();
    const courseContactPhone = (selectedCourse?.celularContactoActividad || '').trim() || '+51 913 452 763';
    const normalizeCountryKey = value => (value || '')
        .trim()
        .toUpperCase()
        .normalize('NFD')
        .replace(/[\u0300-\u036f]/g, '')
        .replace(/\s+/g, '_');
    const normalizeCountryCode = value => {
        const digits = String(value || '').replace(/\D/g, '').slice(0, 5);
        return digits ? `+${digits}` : '';
    };
    const searchableSelects = new WeakMap();

    const normalizeSearchText = value => (value || '')
        .toString()
        .normalize('NFD')
        .replace(/[\u0300-\u036f]/g, '')
        .toLowerCase()
        .trim();

    const refreshSearchableSelect = selectElement => {
        const state = searchableSelects.get(selectElement);
        if (!state) {
            return;
        }

        const selectedOption = selectElement.options[selectElement.selectedIndex];
        state.input.value = selectedOption && selectedOption.value ? selectedOption.textContent.trim() : '';
    };

    const renderSearchableOptions = selectElement => {
        const state = searchableSelects.get(selectElement);
        if (!state) {
            return;
        }

        const query = normalizeSearchText(state.input.value);
        const matches = Array.from(selectElement.options)
            .filter(option => option.value)
            .filter(option => !query || normalizeSearchText(option.textContent).includes(query));

        state.results.innerHTML = '';

        if (matches.length === 0) {
            const empty = document.createElement('div');
            empty.className = 'searchable-select-empty';
            empty.textContent = 'No se encontraron opciones.';
            state.results.appendChild(empty);
            state.results.hidden = false;
            return;
        }

        matches.forEach(option => {
            const optionRow = document.createElement('div');
            optionRow.className = 'searchable-select-option';
            optionRow.setAttribute('role', 'option');
            optionRow.tabIndex = 0;
            optionRow.textContent = option.textContent;
            optionRow.addEventListener('mousedown', event => event.preventDefault());
            const selectOption = () => {
                selectElement.value = option.value;
                refreshSearchableSelect(selectElement);
                state.results.hidden = true;
                selectElement.dispatchEvent(new Event('change', { bubbles: true }));
            };
            optionRow.addEventListener('click', selectOption);
            optionRow.addEventListener('keydown', event => {
                if (event.key === 'Enter' || event.key === ' ') {
                    event.preventDefault();
                    selectOption();
                }
            });
            state.results.appendChild(optionRow);
        });

        state.results.hidden = false;
    };

    const enhanceSearchableSelect = selectElement => {
        if (!selectElement || searchableSelects.has(selectElement)) {
            return;
        }

        const wrapper = document.createElement('div');
        wrapper.className = 'searchable-select';

        const input = document.createElement('input');
        input.type = 'search';
        input.className = 'searchable-select-input';
        input.autocomplete = 'off';
        input.placeholder = selectElement.dataset.searchPlaceholder || 'Buscar...';

        const results = document.createElement('div');
        results.className = 'searchable-select-results';
        results.hidden = true;

        selectElement.parentNode.insertBefore(wrapper, selectElement);
        wrapper.appendChild(input);
        wrapper.appendChild(selectElement);
        wrapper.appendChild(results);

        searchableSelects.set(selectElement, { input, results });

        input.addEventListener('input', () => {
            selectElement.value = '';
            renderSearchableOptions(selectElement);
        });
        input.addEventListener('focus', () => renderSearchableOptions(selectElement));
        input.addEventListener('blur', () => {
            const exact = Array.from(selectElement.options)
                .find(option => option.value && normalizeSearchText(option.textContent) === normalizeSearchText(input.value));
            if (exact) {
                selectElement.value = exact.value;
                selectElement.dispatchEvent(new Event('change', { bubbles: true }));
            }
            refreshSearchableSelect(selectElement);
            window.setTimeout(() => {
                results.hidden = true;
            }, 120);
        });
        input.addEventListener('keydown', event => {
            if (event.key === 'Escape') {
                results.hidden = true;
                input.blur();
            }
        });
        selectElement.addEventListener('change', () => refreshSearchableSelect(selectElement));
        refreshSearchableSelect(selectElement);
    };

    const populateSelect = (selectElement, items, placeholder = 'Seleccione...') => {
        if (!selectElement) {
            return;
        }

        selectElement.innerHTML = '';

        const defaultOption = document.createElement('option');
        defaultOption.value = '';
        defaultOption.textContent = placeholder;
        selectElement.appendChild(defaultOption);

        items.forEach(item => {
            const option = document.createElement('option');
            option.value = item;
            option.textContent = item;
            selectElement.appendChild(option);
        });

        refreshSearchableSelect(selectElement);
    };

    const setCountryCodeValue = (code, countryKey = '') => {
        if (!countryCodeInput) {
            return;
        }

        if (countryCodeInput.tagName === 'SELECT' && countryKey) {
            const exactOption = Array.from(countryCodeInput.options)
                .find(option => option.dataset.country === countryKey && option.value === code);
            if (exactOption) {
                exactOption.selected = true;
                refreshSearchableSelect(countryCodeInput);
                return;
            }
        }

        countryCodeInput.value = code;
        refreshSearchableSelect(countryCodeInput);
    };

    const populateCountryCodeSelect = () => {
        if (!countryCodeInput || countryCodeInput.tagName !== 'SELECT') {
            return;
        }

        countryCodeInput.innerHTML = '';
        countryCallingCodeGroups.forEach(group => {
            const optgroup = document.createElement('optgroup');
            optgroup.label = group.label;

            group.countries.forEach(country => {
                const option = document.createElement('option');
                option.value = country.code;
                option.dataset.country = country.key;
                option.textContent = `${country.name} (${country.code})`;
                optgroup.appendChild(option);
            });

            countryCodeInput.appendChild(optgroup);
        });

        setCountryCodeValue('+51', 'PERU');
        countryCodeInput.dataset.autoSynced = 'true';
        refreshSearchableSelect(countryCodeInput);
    };

    const formatMoney = (value, fallback = 'Por confirmar') => {
        if (!hasValue(value)) {
            return fallback;
        }

        const numericValue = Number(value);
        if (numericValue === 0) {
            return '--';
        }

        return new Intl.NumberFormat('es-PE', {
            style: 'currency',
            currency: 'PEN',
            minimumFractionDigits: 2
        }).format(numericValue);
    };

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

    const parseCourseDate = value => {
        if (!value) {
            return null;
        }

        const isoAttempt = new Date(value);
        if (!Number.isNaN(isoAttempt.getTime())) {
            return isoAttempt;
        }

        const match = String(value).trim().match(/^(\d{1,2})[/-](\d{1,2})[/-](\d{4})$/);
        if (!match) {
            return null;
        }

        const [, day, month, year] = match;
        return new Date(Number(year), Number(month) - 1, Number(day));
    };

    const parseConfiguredDueDates = () => {
        const rawDates = Array.isArray(selectedCourse.fechasPagoCuotas)
            ? selectedCourse.fechasPagoCuotas
            : [];

        return rawDates
            .map(parseCourseDate)
            .filter(date => date instanceof Date && !Number.isNaN(date.getTime()))
            .sort((left, right) => left.getTime() - right.getTime());
    };

    const parseConfiguredInstallmentOptions = () => {
        const rawOptions = Array.isArray(selectedCourse.opcionesCuotas)
            ? selectedCourse.opcionesCuotas
            : [];

        return rawOptions
            .map(option => Number(option))
            .filter(option => Number.isInteger(option) && option > 0 && option <= 24)
            .filter((option, index, options) => options.indexOf(option) === index)
            .sort((left, right) => left - right);
    };

    const parseMoneyValue = value => {
        if (value === null || value === undefined || value === '') {
            return null;
        }

        const parsed = Number(value);
        return Number.isFinite(parsed) ? parsed : null;
    };

    const getBaseCost = () => parseMoneyValue(selectedCourse.costoBase) ?? 0;

    const getInsnsbCost = () => parseMoneyValue(selectedCourse.costoPersonalInsnsb) ?? getBaseCost();

    const courseHasChargeLogic = () => String(selectedCourse.seCobra || '').trim().toUpperCase() === 'SI';

    const calculateFinalCost = () => {
        if (!courseHasChargeLogic()) {
            return 0;
        }

        const isInsnsb = institutionTypeInput.value === 'INSNSB';
        if (isInsnsb) {
            return getInsnsbCost();
        }

        return getBaseCost();
    };

    const roundMoneyToCents = value => Math.round((value || 0) * 100);

    const requiresInsnsbCode = () => {
        if (institutionTypeInput.value !== 'INSNSB') {
            return false;
        }

        if (!courseHasChargeLogic()) {
            return false;
        }

        const baseCost = roundMoneyToCents(getBaseCost());
        const insnsbCost = roundMoneyToCents(getInsnsbCost());
        return baseCost > 0 && baseCost !== insnsbCost;
    };

    const getAllowedInstallmentOptions = () => {
        const configuredDates = parseConfiguredDueDates();
        const configuredOptions = parseConfiguredInstallmentOptions();
        const fallbackMax = Number(selectedCourse.maxCuotas ?? 1);
        const safeFallbackMax = fallbackMax > 0 ? fallbackMax : 1;

        let options = configuredOptions.length > 0
            ? configuredOptions
            : Array.from({ length: safeFallbackMax }, (_, index) => index + 1);

        if (configuredDates.length > 0) {
            options = options.filter(option => option <= configuredDates.length);
        }

        return options.length > 0 ? options : [1];
    };

    const buildRigidInstallmentDates = count => {
        const configuredDates = parseConfiguredDueDates();
        if (configuredDates.length >= count) {
            return configuredDates.slice(0, count);
        }

        const today = new Date();
        today.setHours(0, 0, 0, 0);

        const endDate = parseCourseDate(selectedCourse.fechaFin) || new Date(today.getFullYear(), today.getMonth() + count, today.getDate());
        endDate.setHours(0, 0, 0, 0);

        if (endDate < today) {
            endDate.setTime(today.getTime());
        }

        const firstDueDate = new Date(today);
        firstDueDate.setDate(firstDueDate.getDate() + 2);
        if (firstDueDate > endDate) {
            firstDueDate.setTime(endDate.getTime());
        }

        const dates = [firstDueDate];
        if (count === 1) {
            return dates;
        }

        const daysAvailable = Math.max(0, Math.floor((endDate - firstDueDate) / (24 * 60 * 60 * 1000)));
        const stepDays = Math.max(1, Math.floor(daysAvailable / Math.max(1, count - 1)));

        for (let installment = 2; installment <= count; installment += 1) {
            const dueDate = new Date(firstDueDate);
            dueDate.setDate(firstDueDate.getDate() + stepDays * (installment - 1));

            if (installment === count || dueDate > endDate) {
                dueDate.setTime(endDate.getTime());
            }

            if (dueDate < dates[dates.length - 1]) {
                dueDate.setTime(dates[dates.length - 1].getTime());
            }

            dates.push(dueDate);
        }

        return dates;
    };

    const buildInstallmentSchedule = (total, count) => {
        if (total <= 0 || count <= 0) {
            return [];
        }

        const dueDates = buildRigidInstallmentDates(count);
        const baseAmount = Math.round((total / count) * 100) / 100;
        let accumulated = 0;

        return Array.from({ length: count }, (_, index) => {
            const installmentNumber = index + 1;
            const amount = installmentNumber === count
                ? Math.round((total - accumulated) * 100) / 100
                : baseAmount;

            accumulated += amount;

            return {
                numeroCuota: installmentNumber,
                monto: amount,
                fechaVencimiento: dueDates[index],
                estado: 'PENDIENTE'
            };
        });
    };

    const renderScheduleTable = schedule => {
        paymentScheduleBody.innerHTML = '';

        schedule.forEach(item => {
            const row = document.createElement('tr');
            row.innerHTML = `
                <td data-label="Cuota">${item.numeroCuota}</td>
                <td data-label="Monto">${formatMoney(item.monto, '--')}</td>
                <td data-label="Fecha limite">${item.fechaVencimiento.toLocaleDateString('es-PE')}</td>
                <td data-label="Estado">${item.estado}</td>
            `;
            paymentScheduleBody.appendChild(row);
        });
    };

    const refreshPaymentSummary = () => {
        const finalCost = calculateFinalCost();
        const allowedInstallments = getAllowedInstallmentOptions();
        const requiresPayment = courseHasChargeLogic() && finalCost > 0;

        paymentSummaryCards.forEach(card => {
            card.classList.toggle('hidden-field', !requiresPayment);
        });

        if (internalCost) {
            internalCost.textContent = `Comunicarse al ${courseContactPhone}`;
        }

        if (insnsbCodeHelperText) {
            insnsbCodeHelperText.textContent = requiresInsnsbCode()
                ? `Este codigo es para trabajadores INSNSB, comunicarse al numero ${courseContactPhone} o acercarse a DOCENCIA.`
                : 'Para esta actividad no se requiere codigo INSNSB porque no hay tarifa diferenciada.';
        }

        if (!requiresPayment) {
            paymentPlanSection.classList.add('hidden-field');
            submitButton.textContent = 'Registrar inscripcion';
            cronogramaActual = [];
            paymentScheduleBody.innerHTML = '';
            return;
        }

        submitButton.textContent = 'Continuar inscripción';
        paymentFinalCost.textContent = formatMoney(finalCost, '--');
        paymentMaxInstallments.textContent = allowedInstallments.map(String).join(' o ');
        paymentPlanIntro.textContent = `El costo final es ${formatMoney(finalCost, '--')}. Elige una de estas opciones de cuotas: ${allowedInstallments.map(String).join(' o ')}. Revisa las fechas limite de pago por cuota.`;

        const currentValue = Number(installmentsInput.value || 0);
        const selectedInstallment = allowedInstallments.includes(currentValue)
            ? currentValue
            : allowedInstallments[0];
        installmentsInput.innerHTML = '';
        allowedInstallments.forEach(installment => {
            const option = document.createElement('option');
            option.value = String(installment);
            option.textContent = `${installment} cuota(s)`;
            if (installment === selectedInstallment) {
                option.selected = true;
            }
            installmentsInput.appendChild(option);
        });
        refreshSearchableSelect(installmentsInput);

        cronogramaActual = buildInstallmentSchedule(finalCost, Number(installmentsInput.value));
        renderScheduleTable(cronogramaActual);
    };

    const showPaymentPlan = () => {
        paymentPlanSection.classList.remove('hidden-field');
        refreshPaymentSummary();
        paymentPlanSection.scrollIntoView({ behavior: 'smooth', block: 'start' });
    };

    const hidePaymentPlan = () => {
        paymentPlanSection.classList.add('hidden-field');
    };

    const enableManualNames = () => {
        namesInput.readOnly = false;
        lastNamesInput.readOnly = false;
        namesInput.placeholder = 'Ingrese sus nombres';
        lastNamesInput.placeholder = 'Ingrese sus apellidos';
    };

    const toggleDocumentBehavior = () => {
        if (typeDocumentInput.value === 'DNI') {
            namesInput.readOnly = true;
            lastNamesInput.readOnly = true;
            namesInput.value = '';
            lastNamesInput.value = '';
            namesInput.placeholder = 'Se completara automaticamente';
            lastNamesInput.placeholder = 'Se completara automaticamente';
            documentNumberInput.maxLength = 8;
            documentNumberInput.value = documentNumberInput.value.replace(/\D/g, '').slice(0, 8);
            return;
        }

        enableManualNames();
        documentNumberInput.maxLength = 20;
    };

    const syncRegionValue = () => {
        regionHiddenInput.value = countryInput.value === 'PERU'
            ? regionPeruInput.value
            : regionOutsideInput.value.trim();
    };

    const updateRegionFields = () => {
        const isPeru = countryInput.value === 'PERU';
        regionPeruGroup.classList.toggle('hidden-field', !isPeru);
        regionOutsideGroup.classList.toggle('hidden-field', isPeru);
        regionPeruInput.required = isPeru;
        regionOutsideInput.required = !isPeru;

        if (isPeru) {
            regionOutsideInput.value = '';
        } else {
            regionPeruInput.value = '';
        }
        refreshSearchableSelect(regionPeruInput);

        syncRegionValue();
    };

    const syncCountryCodeFromCountry = (force = false) => {
        if (!countryCodeInput) {
            return;
        }

        const countryKey = normalizeCountryKey(countryInput.value);
        const suggestedCode = countryCallingCodes[countryKey];
        const wasAutoSynced = countryCodeInput.dataset.autoSynced !== 'false';

        if (suggestedCode && (force || wasAutoSynced || !countryCodeInput.value.trim())) {
            setCountryCodeValue(suggestedCode, countryKey);
            countryCodeInput.dataset.autoSynced = 'true';
        }
    };

    const normalizePhoneFields = () => {
        if (countryCodeInput) {
            const normalizedCode = normalizeCountryCode(countryCodeInput.value);
            if (countryCodeInput.tagName !== 'SELECT') {
                countryCodeInput.value = normalizedCode;
            }
            countryCodeInput.setCustomValidity(normalizedCode ? '' : 'Ingresa el codigo de pais. Ejemplo: +51');
        }

        if (phoneInput) {
            const countryDigits = countryCodeInput?.value.replace(/\D/g, '') || '';
            const phoneDigits = phoneInput.value.replace(/\D/g, '');
            phoneInput.value = countryDigits
                && phoneDigits.startsWith(countryDigits)
                && phoneDigits.length > countryDigits.length
                ? phoneDigits.slice(countryDigits.length)
                : phoneDigits;
        }
    };

    const updateSpecialtyFields = () => {
        specialtyInput.required = false;
        specialtyInput.value = '';
        specialtyContainer.classList.add('hidden-field');

        if (isRestrictedDiplomado()) {
            populateSelect(specialtyInput, [restrictedDiplomadoSpecialty]);
            specialtyInput.value = restrictedDiplomadoSpecialty;
            refreshSearchableSelect(specialtyInput);
            specialtyInput.required = true;
            specialtyContainer.classList.remove('hidden-field');
            return;
        }

        if (professionInput.value === 'MEDICINA HUMANA') {
            populateSelect(specialtyInput, especialidadesMedicina);
            specialtyInput.required = true;
            specialtyContainer.classList.remove('hidden-field');
            return;
        }

        if (professionInput.value === 'ENFERMERIA') {
            populateSelect(specialtyInput, especialidadesEnfermeria);
            specialtyInput.required = true;
            specialtyContainer.classList.remove('hidden-field');
            return;
        }
        if (professionInput.value === 'TECNOLOGO MEDICO') {
            populateSelect(specialtyInput, especialidadesTecnologoMedico);
            specialtyInput.required = true;
            specialtyContainer.classList.remove('hidden-field');
            return;
        }
        populateSelect(specialtyInput, []);
    };

    const syncInstitutionName = () => {
        switch (institutionTypeInput.value) {
            case 'IPRESS':
                institutionNameInput.value = ipressInput.value.trim();
                break;
            case 'UNIVERSIDAD':
                institutionNameInput.value = universityInput.value;
                break;
            case 'OTRA':
                institutionNameInput.value = otherInstitutionInput.value.trim();
                break;
            case 'INSNSB':
                institutionNameInput.value = 'INSTITUTO NACIONAL DE SALUD DEL NINO SAN BORJA';
                break;
            default:
                institutionNameInput.value = '';
                break;
        }
    };

    const updateInstitutionFields = () => {
        const institutionType = institutionTypeInput.value;
        const isInsnsb = institutionType === 'INSNSB';
        const shouldRequestInsnsbCode = requiresInsnsbCode();

        ipressContainer.classList.toggle('hidden-field', institutionType !== 'IPRESS');
        universityContainer.classList.toggle('hidden-field', institutionType !== 'UNIVERSIDAD');
        otherInstitutionContainer.classList.toggle('hidden-field', institutionType !== 'OTRA');
        insnsbConditionGroup.classList.toggle('hidden-field', !isInsnsb);
        insnsbCodeGroup.classList.toggle('hidden-field', !shouldRequestInsnsbCode);

        ipressInput.required = institutionType === 'IPRESS';
        universityInput.required = institutionType === 'UNIVERSIDAD';
        otherInstitutionInput.required = institutionType === 'OTRA';
        insnsbConditionInput.required = isInsnsb;
        insnsbCodeInput.required = shouldRequestInsnsbCode;

        if (institutionType !== 'IPRESS') {
            ipressInput.value = '';
        }
        if (institutionType !== 'UNIVERSIDAD') {
            universityInput.value = '';
            refreshSearchableSelect(universityInput);
        }
        if (institutionType !== 'OTRA') {
            otherInstitutionInput.value = '';
        }
        if (!isInsnsb) {
            insnsbConditionInput.value = '';
            refreshSearchableSelect(insnsbConditionInput);
        }
        if (!shouldRequestInsnsbCode) {
            insnsbCodeInput.value = '';
        }

        syncInstitutionName();
        refreshPaymentSummary();
    };

    const updateOtherMediaField = () => {
        const isOther = communicationInput.value === 'OTRO';
        otherMediaGroup.classList.toggle('hidden-field', !isOther);
        otherMediaInput.required = isOther;
        if (!isOther) {
            otherMediaInput.value = '';
        }
    };

    const validateEmailConfirmation = () => {
        if (!emailInput || !emailConfirmInput) {
            return true;
        }

        const mainEmail = normalizeEmail(emailInput.value);
        const confirmEmail = normalizeEmail(emailConfirmInput.value);

        emailInput.value = emailInput.value.trim();
        emailConfirmInput.value = emailConfirmInput.value.trim();

        if (!confirmEmail) {
            emailConfirmInput.setCustomValidity('');
            return false;
        }

        if (mainEmail !== confirmEmail) {
            emailConfirmInput.setCustomValidity('Los correos no coinciden.');
            return false;
        }

        emailConfirmInput.setCustomValidity('');
        return true;
    };

    const consultDni = async dni => {
        if (!appConfig.consultarDniUrl || dni.length !== 8) {
            return;
        }

        namesInput.readOnly = true;
        lastNamesInput.readOnly = true;
        namesInput.value = '';
        lastNamesInput.value = '';
        dniLoader.hidden = false;

        try {
            const response = await fetch(appConfig.consultarDniUrl, {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify({ dni })
            });

            const data = await readJson(response);
            if (!response.ok || !data.success) {
                throw new Error(data.message || 'No se pudo consultar el DNI.');
            }

            namesInput.value = data.nombres || '';
            lastNamesInput.value = data.apellidos || '';
        } catch (error) {
            enableManualNames();
            setFeedback(error.message || 'No se pudo consultar el DNI. Puedes completar los datos manualmente.', 'error');
        } finally {
            dniLoader.hidden = true;
        }
    };

    const showSuccessPanel = data => {
        form.classList.add('hidden-field');
        successPanel.classList.remove('hidden-field');
        successMessage.textContent = data.mensaje || 'Inscripcion registrada correctamente.';
        successTrackingLink.href = data.seguimientoUrl || appConfig.seguimientoUrl || '#';
        successTrackingLink.textContent = data.requierePago
            ? 'Si ya pagaste o deseas revisarlo despues, ir a seguimiento'
            : 'Ir a seguimiento de inscripciones';

        if (successPrimaryLink) {
            if (data.requierePago && data.pagoRedirectUrl) {
                successPrimaryLink.href = data.pagoRedirectUrl;
                successPrimaryLink.textContent = data.pagoRedirectLabel || 'Ir ahora al pago';
                successPrimaryLink.classList.remove('hidden-field');
            } else {
                successPrimaryLink.href = '#';
                successPrimaryLink.textContent = 'Ir ahora al pago';
                successPrimaryLink.classList.add('hidden-field');
            }
        }

        successPanel.scrollIntoView({ behavior: 'smooth', block: 'start' });

        if (data.requierePago && data.pagoRedirectUrl) {
            window.setTimeout(() => {
                window.location.assign(data.pagoRedirectUrl);
            }, 1200);
        }
    };

    const submitRegistration = async () => {
        normalizePhoneFields();

        if (!validateEmailConfirmation()) {
            emailConfirmInput.reportValidity();
            return;
        }

        const payload = Object.fromEntries(new FormData(form).entries());
        const finalCost = calculateFinalCost();
        const requiresPayment = courseHasChargeLogic() && finalCost > 0;

        payload.CursoId = Number(appConfig.selectedCourseId ?? selectedCourse.idActividad ?? 0);
        payload.AceptaTratamientoDatos = dataPolicyInput.checked;
        if (Object.prototype.hasOwnProperty.call(payload, 'AsistiraPresencialPrimerDia')) {
            payload.AsistiraPresencialPrimerDia = payload.AsistiraPresencialPrimerDia === ''
                ? null
                : payload.AsistiraPresencialPrimerDia === 'true';
        }
        payload.CostoFinal = finalCost;
        payload.NumeroCuotas = requiresPayment ? Number(installmentsInput.value || 1) : null;

        submitButton.disabled = true;
        confirmRegistrationButton.disabled = true;
        submitButton.textContent = 'Registrando...';

        try {
            const response = await fetch(appConfig.registrarUrl, {
                method: 'POST',
                headers: {
                    'Accept': 'application/json',
                    'Content-Type': 'application/json'
                },
                body: JSON.stringify(payload)
            });

            const data = await readJson(response);
            if (!response.ok || !data.exito) {
                throw new Error(data.mensaje || 'No se pudo registrar la inscripcion.');
            }

            setFeedback('', 'success');
            showSuccessPanel(data);
        } catch (error) {
            setFeedback(error.message || 'Ocurrio un error al registrar la inscripcion.', 'error');
        } finally {
            submitButton.disabled = false;
            confirmRegistrationButton.disabled = false;
            refreshPaymentSummary();
        }
    };

    populateSelect(countryInput, paises);
    populateSelect(regionPeruInput, departamentosPeru);
    populateSelect(professionInput, isRestrictedDiplomado() ? [restrictedDiplomadoProfession] : profesiones);
    populateSelect(specialtyInput, []);
    populateSelect(universityInput, universidades);
    populateCountryCodeSelect();
    document.querySelectorAll('select[data-searchable-select]').forEach(enhanceSearchableSelect);

    countryInput.value = 'PERU';
    if (isRestrictedDiplomado()) {
        professionInput.value = restrictedDiplomadoProfession;
    }
    document.querySelectorAll('select[data-searchable-select]').forEach(refreshSearchableSelect);

    typeDocumentInput.addEventListener('change', toggleDocumentBehavior);
    documentNumberInput.addEventListener('input', () => {
        if (typeDocumentInput.value === 'DNI') {
            documentNumberInput.value = documentNumberInput.value.replace(/\D/g, '').slice(0, 8);
            if (documentNumberInput.value.length === 8) {
                consultDni(documentNumberInput.value);
            }
        }
    });

    countryInput.addEventListener('change', () => {
        updateRegionFields();
        syncCountryCodeFromCountry();
    });
    regionPeruInput.addEventListener('change', syncRegionValue);
    regionOutsideInput.addEventListener('input', syncRegionValue);
    countryCodeInput?.addEventListener('input', () => {
        countryCodeInput.dataset.autoSynced = 'false';
        if (countryCodeInput.tagName !== 'SELECT') {
            countryCodeInput.value = normalizeCountryCode(countryCodeInput.value);
        }
    });
    countryCodeInput?.addEventListener('change', () => {
        countryCodeInput.dataset.autoSynced = 'false';
        normalizePhoneFields();
    });
    countryCodeInput?.addEventListener('blur', normalizePhoneFields);
    phoneInput?.addEventListener('blur', normalizePhoneFields);
    emailInput?.addEventListener('input', validateEmailConfirmation);
    emailConfirmInput?.addEventListener('input', validateEmailConfirmation);
    emailConfirmInput?.addEventListener('blur', validateEmailConfirmation);
    professionInput.addEventListener('change', updateSpecialtyFields);
    institutionTypeInput.addEventListener('change', updateInstitutionFields);
    ipressInput.addEventListener('input', syncInstitutionName);
    universityInput.addEventListener('change', syncInstitutionName);
    otherInstitutionInput.addEventListener('input', syncInstitutionName);
    communicationInput.addEventListener('change', updateOtherMediaField);
    installmentsInput.addEventListener('change', () => {
        cronogramaActual = buildInstallmentSchedule(calculateFinalCost(), Number(installmentsInput.value || 1));
        renderScheduleTable(cronogramaActual);
    });

    backToFormButton.addEventListener('click', hidePaymentPlan);
    confirmRegistrationButton.addEventListener('click', () => {
        setFeedback('', 'success');
        submitRegistration();
    });

    form.addEventListener('submit', async event => {
        event.preventDefault();
        setFeedback('', 'success');
        syncRegionValue();
        syncInstitutionName();
        normalizePhoneFields();
        validateEmailConfirmation();

        if (!form.checkValidity()) {
            form.reportValidity();
            setFeedback('Completa los campos obligatorios antes de continuar.', 'error');
            return;
        }

        const finalCost = calculateFinalCost();
        if (courseHasChargeLogic() && finalCost > 0) {
            showPaymentPlan();
            return;
        }

        await submitRegistration();
    });

    toggleDocumentBehavior();
    updateRegionFields();
    syncCountryCodeFromCountry(true);
    normalizePhoneFields();
    updateSpecialtyFields();
    updateInstitutionFields();
    updateOtherMediaField();
    validateEmailConfirmation();
    refreshPaymentSummary();
});
