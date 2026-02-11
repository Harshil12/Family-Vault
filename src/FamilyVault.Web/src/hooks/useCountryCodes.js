import React, { useEffect, useState } from "react";

const CACHE_KEY = "familyvault_country_codes_v1";

const fallbackOptions = [
  { label: "United States (+1)", value: "+1" },
  { label: "India (+91)", value: "+91" },
  { label: "United Kingdom (+44)", value: "+44" },
  { label: "Canada (+1)", value: "+1" },
  { label: "Australia (+61)", value: "+61" }
];

function mapCountriesToOptions(countries) {
  const mapped = [];

  for (const country of countries) {
    const name = country?.name?.common;
    const root = country?.idd?.root;
    const suffixes = country?.idd?.suffixes;

    if (!name || !root) {
      continue;
    }

    if (Array.isArray(suffixes) && suffixes.length) {
      for (const suffix of suffixes) {
        const code = `${root}${suffix}`;
        mapped.push({
          label: `${name} (${code})`,
          value: code
        });
      }
    } else {
      mapped.push({
        label: `${name} (${root})`,
        value: root
      });
    }
  }

  const unique = Array.from(new Map(mapped.map((item) => [`${item.label}|${item.value}`, item])).values());
  unique.sort((a, b) => a.label.localeCompare(b.label));
  return unique;
}

export default function useCountryCodes() {
  const [options, setOptions] = useState(() => {
    try {
      const cached = localStorage.getItem(CACHE_KEY);
      if (!cached) {
        return fallbackOptions;
      }
      const parsed = JSON.parse(cached);
      return Array.isArray(parsed) && parsed.length ? parsed : fallbackOptions;
    } catch {
      return fallbackOptions;
    }
  });

  useEffect(() => {
    let isMounted = true;
    const controller = new AbortController();

    const load = async () => {
      try {
        const response = await fetch("https://restcountries.com/v3.1/all?fields=name,idd,cca2", {
          signal: controller.signal
        });

        if (!response.ok) {
          return;
        }

        const data = await response.json();
        const nextOptions = mapCountriesToOptions(data);
        if (!nextOptions.length || !isMounted) {
          return;
        }

        setOptions(nextOptions);
        localStorage.setItem(CACHE_KEY, JSON.stringify(nextOptions));
      } catch {
        // Keep fallback/cached options if network fails.
      }
    };

    load();

    return () => {
      isMounted = false;
      controller.abort();
    };
  }, []);

  return options;
}
