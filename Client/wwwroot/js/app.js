function pageLoading(params) {
    if (params.isLoading) {
        document.querySelector('body').classList.add('overflow-hidden');
    }
    else {
        document.querySelector('body').classList.remove('overflow-hidden');
    }
}