(function () {
  window.initializeImageInputPreview = function (options) {
    const uploadInput = document.getElementById(options.uploadInputId);
    const cameraInput = document.getElementById(options.cameraInputId);
    const previewContainer = document.getElementById(
      options.previewContainerId,
    );
    const previewImage = document.getElementById(options.previewImageId);
    const previewText = document.getElementById(options.previewTextId);

    if (!uploadInput || !cameraInput || !previewContainer || !previewImage) {
      return;
    }

    let currentObjectUrl = null;

    function updatePreview(sourceInput, otherInput) {
      const file = sourceInput.files && sourceInput.files[0];
      if (!file) {
        return;
      }

      otherInput.value = "";

      if (currentObjectUrl) {
        URL.revokeObjectURL(currentObjectUrl);
      }

      currentObjectUrl = URL.createObjectURL(file);
      previewImage.src = currentObjectUrl;
      previewContainer.classList.remove("d-none");

      if (previewText) {
        previewText.textContent = file.name;
      }
    }

    uploadInput.addEventListener("change", function () {
      updatePreview(uploadInput, cameraInput);
    });

    cameraInput.addEventListener("change", function () {
      updatePreview(cameraInput, uploadInput);
    });
  };
})();
