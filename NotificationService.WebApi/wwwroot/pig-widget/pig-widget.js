(() => {
  const root = document.getElementById('pigEasterEgg');
  const button = document.getElementById('pigButton');
  const sound = document.getElementById('pigSound');
  const counter = document.getElementById('pigCounter');

  if (!root || !button || !sound || !counter) return;

  const storageKey = 'orderflow-pig-oinks';
  let oinks = Number.parseInt(localStorage.getItem(storageKey) ?? '0', 10);

  if (!Number.isFinite(oinks) || oinks < 0) oinks = 0;

  const renderCounter = () => {
    counter.textContent = `Хрюков: ${oinks}`;
  };

  const animate = () => {
    button.classList.remove('is-oinking');
    void button.offsetWidth;
    button.classList.add('is-oinking');
  };

  const playOink = async () => {
    sound.pause();
    sound.currentTime = 0;

    try {
      await sound.play();
    } catch {
      return;
    }

    oinks += 1;
    localStorage.setItem(storageKey, String(oinks));
    renderCounter();
    animate();
  };

  button.addEventListener('click', playOink);
  button.addEventListener('animationend', () => button.classList.remove('is-oinking'));

  renderCounter();
})();
