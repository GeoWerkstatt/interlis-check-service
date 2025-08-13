(function () {
  window.addEventListener("load", function () {
    setTimeout(function () {
      // Override favicon
      var link = document.createElement('link');
      link.rel = 'icon';
      link.type = 'image/png';
      link.href = '../favicon.ico';
      link.sizes = '16x16 32x32';
      document.getElementsByTagName('head')[0].appendChild(link);
    });
  });
})();
