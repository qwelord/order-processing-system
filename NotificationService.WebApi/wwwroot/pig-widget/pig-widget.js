(() => {
    const button = document.getElementById('pigButton');
    const sound = document.getElementById('pigSound');
    const counter = document.getElementById('pigCounter');

    if (!button || !counter) {
        console.error('Pig widget: required elements not found');
        return;
    }

    let oinks = 0;

    const renderCounter = () => {
        counter.textContent = `\u0425\u0440\u044e: ${oinks}`;
    };

    const animate = () => {
        button.classList.remove('is-oinking');

        void button.offsetWidth;

        button.classList.add('is-oinking');
    };

    const playOink = () => {
        oinks += 1;

        renderCounter();
        animate();

        if (!sound) return;

        sound.pause();
        sound.currentTime = 0;

        sound.play().catch(error => {
            console.warn('Pig sound could not be played:', error);
        });
    };

    button.addEventListener('click', playOink);

    button.addEventListener('animationend', () => {
        button.classList.remove('is-oinking');
    });

    renderCounter();
})();