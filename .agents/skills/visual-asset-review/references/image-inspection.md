# Inspecting the delivered file

## Provenance
- Record path, dimensions and available revision/source; a hash helps distinguish versions when needed.
- After downloading, open the file and check the subject. Alt-text, extension, positive file size and
  the last page element do not prove the image corresponds to the current generation.
- Use exposed tools and current session documentation. Do not assume browser APIs,
  scratchpad scripts, authenticated fetch downloads or a Downloads path.

## Alpha and cropping
- Verify alpha channel presence and value distribution with an available pixel reader.
  An RGBA file can be fully opaque; a checkerboard in the preview may be painted into RGB.
- Sample the background and subject edges; composite over contrasting backgrounds when the tool allows.
- Look for halos, holes in light clothing, cropped shadows and neighboring-cell pixels.
- Do not globally remove every light color. Background removal must preserve subject colors
  and be validated in the resulting file. A threshold from one batch does not apply to all.
- For audit-only review, use read/preview; if a composite is needed, request that artifact from
  the executor or use an in-memory preview without modifying the reviewed asset.

## Scale and repetition
- Compare base and revision at the same zoom with visible occupancy measurements, not just canvas size.
- Tiles require inspecting horizontal/vertical seams in repetition; isolated objects require margins and complete silhouettes.
- Report native-resolution sharpness separately from readability at game scale.
- Correct format does not prove artistic acceptance; a visual opinion does not prove import/runtime.
