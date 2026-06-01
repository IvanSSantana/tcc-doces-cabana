const slides = document.querySelectorAll('.slide');
const dots = document.querySelectorAll('.dot');
const btnPrev = document.querySelector('.prev');
const btnNext = document.querySelector('.next');

let indice = 0;

function mostrarSlide(n){

    slides.forEach(slide =>
        slide.classList.remove('ativo')
    );

    dots.forEach(dot =>
        dot.classList.remove('ativo')
    );

    slides[n].classList.add('ativo');
    dots[n].classList.add('ativo');
}

function proximo(){
    indice++;
    if(indice >= slides.length){
        indice = 0;
    }
    mostrarSlide(indice);
}

function anterior(){
    indice--;
    if(indice < 0){
        indice = slides.length - 1;
    }
    mostrarSlide(indice);
}

btnNext.addEventListener('click', proximo);
btnPrev.addEventListener('click', anterior);

dots.forEach((dot, i) => {
    dot.addEventListener('click', () => {
        indice = i;
        mostrarSlide(indice);
    });
});

// autoplay
setInterval(proximo, 5000);